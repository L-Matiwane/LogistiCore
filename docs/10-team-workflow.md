# Team Workflow

## Branching (GitHub Flow)
- `main`: always deployable/demoable, protected. No direct pushes.
- Branch names: `feature/FR-A1-register-parcel`, `fix/route-capacity-check`, `docs/erd-update`.
- Short-lived branches (1-3 days), rebase or merge `main` often.

## Commits (Conventional Commits)
`feat(parcels): generate tracking number`, `fix(routes): block double booking`, `docs: update ERD`, `test(billing): invoice VAT`, `chore: bump packages`.

## Pull requests
1. Link the issue (`Closes #12`).
2. CI green (build + tests).
3. At least one reviewer who did not write it.
4. Screenshots for UI changes.
5. Squash and merge.

## Code standards
- C#: PascalCase types/methods, `_camelCase` private fields, async suffix, nullable reference types on, no business logic in controllers.
- React: function components + hooks, TypeScript strict, one feature folder per module.
- SQL: PascalCase tables, plural nouns, explicit constraint names.
- No secrets in Git. Use user-secrets/env vars.

## Branch protection (GitHub → Settings → Branches)
Require PR, 1 approval, status checks (CI) pass, no force-push.

## Definition of Ready (issue)
Has user story, acceptance criteria, requirement ID, estimate.

## Portfolio tips
- Keep commits and PRs under your own name so your contribution is visible to employers.
- Pin the repo on your GitHub profile; write a strong README with screenshots and a demo GIF/video.
- Write a short "What I built" section describing your part (e.g. dispatch module, billing, auth).
- Seed realistic demo data and include demo logins in the README.
