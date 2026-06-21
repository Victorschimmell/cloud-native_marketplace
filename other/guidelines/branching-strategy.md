## 🌿 Branching Strategy

`feature/* → release/v* → master`

### 🔒 Branch Roles
- **master** – Production (source of truth)  
- **release** – Pre-prod, culmination of work in a sprint. Merge into master after each sprint.
- **feature/bug/hotfix..** - Actively working branches with commit-freedom. 

### 🔄 Flow
1. Branch out from current `release` → `feature/{US-NUMBER}/{DESCIPTION}` (can be `feature`, `bug`, `hotfix` or similar) and make your changes on this branch.
2. When you're done, now create a PR against the current `release`-branch merging `feature/{US-NUMBER}/{DESCIPTION}` into `release`.
3. When the `release`-branch has all our changes from the current sprint and we're done, we create a PR from `release` → `master` (simulating prod).

### ⚖️ Rules
- Protected: `master`, `release`  
- No direct commits to the above.
- Always flow: **feature (or similar) → release → master**

### 📝 Example
1. I create branch `feature/25/My-changes-here` based on current `release/v1.0.0` (for user-story 25 in this example).
2. I now do my changes, test the system and want to conclude my work by creating a PR into the release-branch: PR now created trying to merge `feature/25/My-changes-here` into `release/v1.0.0`.
3. When PR is completed, my changes are now in `release/v1.0.0`.
4. Let's say the sprint is done now, and we're happy with our changes. Now we simulate deploying our release to prod by merging `release/v1.0.0` into `master`.
5. When PR is completed, we have now deployed our `release/v1.0.0` to prod (master).