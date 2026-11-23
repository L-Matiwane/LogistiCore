<#
  LogistiCore smoke test
  Runs the main journey against a RUNNING API and prints PASS or FAIL for each check.

  Before you run it:
    1. Start the API (dotnet run in backend\src\LogistiCore.Api)
    2. In this PowerShell window run:  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
    3. Then run:  .\scripts\smoke-test.ps1

  It creates a few test parcels each time you run it.
#>
param(
    [string]$BaseUrl = 'http://localhost:5027',
    [string]$Password = 'LogistiCore#2026'
)

$script:Passed = 0
$script:Failed = 0

function Invoke-Api {
    param([string]$Method = 'GET', [string]$Path, $Body = $null, [string]$Token = $null)

    $headers = @{}
    if ($Token) { $headers['Authorization'] = "Bearer $Token" }

    $params = @{ Method = $Method; Uri = "$BaseUrl$Path"; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $Body) {
        $params['Body'] = ($Body | ConvertTo-Json -Depth 6)
        $params['ContentType'] = 'application/json'
    }

    try {
        $response = Invoke-WebRequest @params
        $data = $null
        if ($response.Content) { $data = $response.Content | ConvertFrom-Json }
        return [pscustomobject]@{ Status = [int]$response.StatusCode; Data = $data }
    }
    catch {
        $status = 0
        $data = $null
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $text = $reader.ReadToEnd()
                if ($text) { $data = $text | ConvertFrom-Json }
            } catch { }
        }
        return [pscustomobject]@{ Status = $status; Data = $data }
    }
}

function Check {
    param([string]$Name, [bool]$Condition, [string]$Detail = '')
    if ($Condition) {
        $script:Passed++
        Write-Host ("  PASS  " + $Name) -ForegroundColor Green
    } else {
        $script:Failed++
        Write-Host ("  FAIL  " + $Name + $(if ($Detail) { "  ->  $Detail" } else { '' })) -ForegroundColor Red
    }
}

function Login([string]$Email) {
    $r = Invoke-Api -Method POST -Path '/api/v1/auth/login' -Body @{ email = $Email; password = $Password }
    return $r
}

function New-ParcelBody([int]$CustomerId, [decimal]$Weight, [string]$Service) {
    return @{
        customerId = $CustomerId; recipientName = 'Smoke Test'; recipientPhone = '0821234567'
        recipientEmail = $null; deliveryAddress = '1 Test Street'; city = 'East London'; postalCode = '5201'
        deliveryInstructions = $null; weightKg = $Weight; lengthCm = $null; widthCm = $null; heightCm = $null
        serviceLevel = $Service; specialHandling = @('Fragile')
    }
}

Write-Host ''
Write-Host 'LogistiCore smoke test' -ForegroundColor Cyan
Write-Host ("API: " + $BaseUrl)

# ---------------------------------------------------------------- health
Write-Host "`n1. Health"
$health = Invoke-Api -Path '/health/db'
Check 'API is reachable and the database is connected' ($health.Status -eq 200 -and $health.Data.database -eq 'connected') ("status " + $health.Status)
if ($health.Status -ne 200) {
    Write-Host "`nThe API is not answering. Start it first, then run this script again." -ForegroundColor Yellow
    exit 1
}

# ---------------------------------------------------------------- security
Write-Host "`n2. Login and security"
$admin = Login 'admin@logisticore.test'
$clerk = Login 'clerk@logisticore.test'
$dispatcher = Login 'dispatcher@logisticore.test'
$driver = Login 'driver1@logisticore.test'
Check 'Admin can log in' ($admin.Status -eq 200 -and $admin.Data.role -eq 'Admin') ("status " + $admin.Status)
Check 'Clerk can log in' ($clerk.Status -eq 200 -and $clerk.Data.role -eq 'WarehouseClerk')
Check 'Dispatcher can log in' ($dispatcher.Status -eq 200 -and $dispatcher.Data.role -eq 'Dispatcher')
Check 'Driver can log in' ($driver.Status -eq 200 -and $driver.Data.role -eq 'Driver')
$bad = Invoke-Api -Method POST -Path '/api/v1/auth/login' -Body @{ email = 'admin@logisticore.test'; password = 'wrong-password' }
Check 'Wrong password is refused (401)' ($bad.Status -eq 401)
$noToken = Invoke-Api -Path '/api/v1/parcels'
Check 'Parcel list without a token is refused (401)' ($noToken.Status -eq 401)
$driverList = Invoke-Api -Path '/api/v1/parcels' -Token $driver.Data.token
Check 'A driver cannot list parcels (403)' ($driverList.Status -eq 403)
$adminPing = Invoke-Api -Path '/api/v1/admin/ping' -Token $clerk.Data.token
Check 'A clerk cannot use admin-only endpoints (403)' ($adminPing.Status -eq 403)

$clerkToken = $clerk.Data.token
$dispatcherToken = $dispatcher.Data.token
$adminToken = $admin.Data.token

# ---------------------------------------------------------------- pricing
Write-Host "`n3. Pricing"
function Quote($Weight, $Service, $L = $null, $W = $null, $H = $null) {
    return Invoke-Api -Method POST -Path '/api/v1/parcels/quote' -Token $clerkToken -Body @{
        weightKg = $Weight; lengthCm = $L; widthCm = $W; heightCm = $H; serviceLevel = $Service }
}
$q1 = Quote 1.5 'Standard'
Check 'Standard 1.5 kg costs R60' ($q1.Status -eq 200 -and $q1.Data.price -eq 60) ("got " + $q1.Data.price)
$q2 = Quote 1.5 'Express'
Check 'Express 1.5 kg costs R95' ($q2.Status -eq 200 -and $q2.Data.price -eq 95) ("got " + $q2.Data.price)
$q3 = Quote 2 'Standard'
Check 'Exactly 2 kg is still in the first band (R60)' ($q3.Status -eq 200 -and $q3.Data.price -eq 60) ("got " + $q3.Data.price)
$q4 = Quote 2.01 'Standard'
Check '2.01 kg moves to the next band (R95)' ($q4.Status -eq 200 -and $q4.Data.price -eq 95) ("got " + $q4.Data.price)
$q5 = Quote 4.2 'Standard' 40 35 20
Check 'Volumetric weight 5.6 kg is billed instead of 4.2 kg' ($q5.Status -eq 200 -and $q5.Data.billedWeightKg -eq 5.6 -and $q5.Data.price -eq 95) ("got billed " + $q5.Data.billedWeightKg)
$q6 = Quote 45 'Standard'
Check '45 kg is rejected (400)' ($q6.Status -eq 400)
$q7 = Quote 3 'Turbo'
Check 'Unknown service level is rejected (400)' ($q7.Status -eq 400)

# ---------------------------------------------------------------- registration
Write-Host "`n4. Register parcels"
$customers = Invoke-Api -Path '/api/v1/customers' -Token $clerkToken
Check 'Customer list loads' ($customers.Status -eq 200 -and @($customers.Data).Count -ge 1)
$customerId = @($customers.Data)[0].customerId

$p1 = Invoke-Api -Method POST -Path '/api/v1/parcels' -Token $clerkToken -Body (New-ParcelBody $customerId 1.5 'Standard')
Check 'Clerk can register a parcel (201)' ($p1.Status -eq 201) ("status " + $p1.Status)
$p2 = Invoke-Api -Method POST -Path '/api/v1/parcels' -Token $clerkToken -Body (New-ParcelBody $customerId 3 'Express')
Check 'A second parcel can be registered' ($p2.Status -eq 201)
Check 'Tracking number has the format FCL-YYYYMMDD-NNNNNN' ($p1.Data.trackingNo -match '^FCL-\d{8}-\d{6}$') ("got " + $p1.Data.trackingNo)
$seq1 = [int]($p1.Data.trackingNo.Substring($p1.Data.trackingNo.Length - 6))
$seq2 = [int]($p2.Data.trackingNo.Substring($p2.Data.trackingNo.Length - 6))
Check 'The tracking sequence counts up by one' ($seq2 -eq $seq1 + 1) ("$seq1 then $seq2")
Check 'New parcel starts as Registered' ($p1.Data.currentStatus -eq 'Registered')
Check 'Price was taken from the rate card (R60)' ($p1.Data.price -eq 60)
Check 'Status history has the first entry' (@($p1.Data.history).Count -eq 1)
$noPhone = New-ParcelBody $customerId 1 'Standard'
$noPhone.recipientPhone = ''
$p3 = Invoke-Api -Method POST -Path '/api/v1/parcels' -Token $clerkToken -Body $noPhone
Check 'Missing phone number is rejected (400)' ($p3.Status -eq 400)
$p4 = Invoke-Api -Method POST -Path '/api/v1/parcels' -Token $dispatcherToken -Body (New-ParcelBody $customerId 1 'Standard')
Check 'A dispatcher cannot register parcels (403)' ($p4.Status -eq 403)

$track1 = $p1.Data.trackingNo
$track2 = $p2.Data.trackingNo
$parcelId1 = $p1.Data.parcelId

# ---------------------------------------------------------------- search
Write-Host "`n5. Search and detail"
$found = Invoke-Api -Path ("/api/v1/parcels?q=" + $track1) -Token $adminToken
Check 'Searching by tracking number finds the parcel' ($found.Status -eq 200 -and $found.Data.totalCount -eq 1) ("count " + $found.Data.totalCount)
$byStatus = Invoke-Api -Path '/api/v1/parcels?status=Registered' -Token $adminToken
Check 'Filtering by status works' ($byStatus.Status -eq 200 -and $byStatus.Data.totalCount -ge 2)
$detail = Invoke-Api -Path "/api/v1/parcels/$parcelId1" -Token $adminToken
Check 'Parcel detail loads' ($detail.Status -eq 200 -and $detail.Data.trackingNo -eq $track1)
$missing = Invoke-Api -Path '/api/v1/parcels/99999999' -Token $adminToken
Check 'Unknown parcel returns 404' ($missing.Status -eq 404)

# ---------------------------------------------------------------- warehouse
Write-Host "`n6. Warehouse"
$bins = Invoke-Api -Path '/api/v1/warehouse/bins' -Token $clerkToken
Check 'Bins load' ($bins.Status -eq 200 -and @($bins.Data).Count -ge 1)
$binA = @($bins.Data) | Where-Object { $_.code -eq 'A-01' } | Select-Object -First 1
$before = $binA.occupied

$sortEarly = Invoke-Api -Method POST -Path '/api/v1/warehouse/sort' -Token $clerkToken -Body @{ trackingNo = $track1; binId = $binA.binId }
Check 'A parcel that was not received cannot be sorted (422)' ($sortEarly.Status -eq 422)

$recv = Invoke-Api -Method POST -Path '/api/v1/warehouse/receive' -Token $clerkToken -Body @{ trackingNo = $track1 }
Check 'Receive moves the parcel to AtWarehouse' ($recv.Status -eq 200 -and $recv.Data.currentStatus -eq 'AtWarehouse') ("status " + $recv.Status)
$recvAgain = Invoke-Api -Method POST -Path '/api/v1/warehouse/receive' -Token $clerkToken -Body @{ trackingNo = $track1 }
Check 'Receiving twice is rejected (422)' ($recvAgain.Status -eq 422)
$unknown = Invoke-Api -Method POST -Path '/api/v1/warehouse/receive' -Token $clerkToken -Body @{ trackingNo = 'FCL-00000000-000000' }
Check 'Unknown tracking number is rejected (422)' ($unknown.Status -eq 422)

$sort = Invoke-Api -Method POST -Path '/api/v1/warehouse/sort' -Token $clerkToken -Body @{ trackingNo = $track1; binId = $binA.binId }
Check 'Sort moves the parcel to Sorted in bin A-01' ($sort.Status -eq 200 -and $sort.Data.currentStatus -eq 'Sorted' -and $sort.Data.binCode -eq 'A-01') ("status " + $sort.Status)
$sortAgain = Invoke-Api -Method POST -Path '/api/v1/warehouse/sort' -Token $clerkToken -Body @{ trackingNo = $track1; binId = $binA.binId }
Check 'Sorting into the same bin again is rejected (422)' ($sortAgain.Status -eq 422)
$bins2 = Invoke-Api -Path '/api/v1/warehouse/bins' -Token $clerkToken
$binA2 = @($bins2.Data) | Where-Object { $_.code -eq 'A-01' } | Select-Object -First 1
Check 'Bin A-01 occupancy went up by one' ($binA2.occupied -eq $before + 1) ("before $before, after " + $binA2.occupied)

$exc = Invoke-Api -Method POST -Path '/api/v1/warehouse/exceptions' -Token $clerkToken -Body @{ trackingNo = $track2; type = 'Damaged'; reason = 'Smoke test: box crushed' }
Check 'A Registered parcel can be flagged Damaged' ($exc.Status -eq 200 -and $exc.Data.currentStatus -eq 'Damaged') ("status " + $exc.Status)
$excAgain = Invoke-Api -Method POST -Path '/api/v1/warehouse/exceptions' -Token $clerkToken -Body @{ trackingNo = $track2; type = 'Lost'; reason = 'Again' }
Check 'A damaged parcel cannot be flagged again (422)' ($excAgain.Status -eq 422)
$excNoReason = Invoke-Api -Method POST -Path '/api/v1/warehouse/exceptions' -Token $clerkToken -Body @{ trackingNo = $track1; type = 'Lost'; reason = '' }
Check 'An exception needs a reason (400)' ($excNoReason.Status -eq 400)
$excList = Invoke-Api -Path '/api/v1/warehouse/exceptions' -Token $adminToken
Check 'The exception appears in the list' ($excList.Status -eq 200 -and (@($excList.Data) | Where-Object { $_.trackingNo -eq $track2 }))
$inv = Invoke-Api -Path ("/api/v1/warehouse/inventory?binId=" + $binA.binId) -Token $adminToken
Check 'Inventory for bin A-01 lists the sorted parcel' ($inv.Status -eq 200 -and (@($inv.Data) | Where-Object { $_.trackingNo -eq $track1 }))

# ---------------------------------------------------------------- history + dashboard
Write-Host "`n7. History and dashboard"
$detail2 = Invoke-Api -Path "/api/v1/parcels/$parcelId1" -Token $adminToken
$statuses = (@($detail2.Data.history) | ForEach-Object { $_.status }) -join ','
Check 'History shows Registered, AtWarehouse, Sorted in order' ($statuses -eq 'Registered,AtWarehouse,Sorted') ("got " + $statuses)
$firstEntry = @($detail2.Data.history)[0]
Check 'History records who made the change' ($firstEntry.changedByName -eq 'Thandi Clerk') ("got " + $firstEntry.changedByName)
$summary = Invoke-Api -Path '/api/v1/dashboard/summary' -Token $adminToken
Check 'Dashboard summary loads' ($summary.Status -eq 200)
Check 'Dashboard counts parcels registered today' ($summary.Data.parcelsToday -ge 2) ("got " + $summary.Data.parcelsToday)
Check 'Dashboard counts the exception' ($summary.Data.exceptions -ge 1)
Check 'Weekly volume has 7 days' (@($summary.Data.weeklyVolume).Count -eq 7)

# ---------------------------------------------------------------- result
Write-Host ''
Write-Host ("Passed: " + $script:Passed + "   Failed: " + $script:Failed) -ForegroundColor $(if ($script:Failed -eq 0) { 'Green' } else { 'Red' })
if ($script:Failed -gt 0) { exit 1 }
