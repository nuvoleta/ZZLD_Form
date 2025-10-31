# Git Standards and Workflow

## Repository Structure

### Protected Directories
The following directories are excluded from version control via `.gitignore`:
- `client_secret/` - Contains sensitive API keys and credentials
- `videos/` - Contains large video files that should not be tracked
- `YT_downloader/` - Contains downloaded content that should not be tracked

### Repository Organization
- **Feature-based structure**: Organize code by features, not file types
- **Co-located tests**: Test files should be near the code they test
- **Documentation**: Keep documentation current and organized in `/documentation/`

## Commit Standards

### Commit Message Guidelines
- **Descriptive messages**: Explain the "why" not just the "what"
- **Atomic commits**: Each commit should represent one logical change
- **Imperative mood**: Use present tense, imperative mood (e.g., "Add feature" not "Added feature")
- **Scope clarity**: Include affected component/feature in commit message

### Commit Message Format
```
<type>(<scope>): <description>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(auth): add Azure AD federated logout

Implement proper federated logout following OIDC specifications.
Includes provider-specific logout flows and session cleanup.

Resolves: #25
```

## Branch Strategy

### Branch Naming Conventions
- **Feature branches**: `feature/description-of-feature`
- **Bug fixes**: `fix/description-of-bug`
- **Hotfixes**: `hotfix/critical-issue-description`
- **Release branches**: `release/version-number`

### Branch Rules
- **Master branch**: Always stable and deployable (Note: this repo uses `master`, not `main`)
- **No direct pushes**: Never push directly to master branch
- **Feature branches**: Create from master for all new development
- **Descriptive names**: Use clear, descriptive branch names

### Examples
```bash
# Good branch names
feature/user-management-dashboard
fix/video-upload-authentication
hotfix/critical-security-patch

# Poor branch names  
feature/stuff
fix/bug
update
```

## Pull Request Workflow

### Creating Pull Requests

1. **Create feature branch**:
   ```bash
   git checkout -b feature/description
   ```

2. **Make changes and commit**:
   ```bash
   git add .
   git commit -m "descriptive message"
   ```

3. **Push feature branch**:
   ```bash
   git push -u origin feature/description
   ```

4. **Create pull request**:
   ```bash
   gh pr create --title "Title" --body "Description"
   ```

### Pull Request Requirements

#### Before Creating PR
- [ ] All tests pass locally
- [ ] Code follows project style guidelines
- [ ] Documentation updated if needed
- [ ] No sensitive information in commits

#### PR Description Template
```markdown
## Summary
Brief description of changes

## Changes Made
- List of specific changes
- Technical details
- Breaking changes (if any)

## Testing
- Unit tests added/updated
- Integration tests verified
- Manual testing performed

## Checklist
- [ ] Tests pass
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] No breaking changes (or properly documented)
```

#### Review Process
- **Code review required**: At least one approval needed
- **Automated checks**: All CI/CD checks must pass
- **Testing verification**: QA testing for significant changes
- **Documentation review**: Ensure docs are updated

#### Merging
- **Merge strategy**: Squash and merge for clean history
- **Delete branch**: Remove feature branch after merge
- **Deploy verification**: Verify deployment after merge

## Commands Reference

### Basic Git Operations
```bash
# Initialize repository
git init

# Clone repository
git clone <repository-url>

# Check status
git status

# Add files
git add .
git add <specific-file>

# Commit changes
git commit -m "descriptive message"

# Push changes
git push origin <branch-name>
```

### Branch Operations
```bash
# Create and switch to new branch
git checkout -b feature-name

# Switch branches
git checkout branch-name

# List branches
git branch -a

# Delete local branch
git branch -d branch-name

# Delete remote branch
git push origin --delete branch-name
```

### GitHub CLI Operations
```bash
# Create pull request
gh pr create --title "Title" --body "Description"

# List pull requests
gh pr list

# View PR status
gh pr status

# Merge pull request
gh pr merge <pr-number>

# Check out PR locally
gh pr checkout <pr-number>
```

### Advanced Operations
```bash
# Interactive rebase (clean up commits)
git rebase -i HEAD~3

# Cherry-pick specific commit
git cherry-pick <commit-hash>

# Stash changes
git stash
git stash pop

# View commit history
git log --oneline --graph

# Reset to specific commit
git reset --hard <commit-hash>
```

## Quality Gates

### Pre-commit Checks
- **Linting**: ESLint and Prettier checks
- **Type checking**: TypeScript compilation
- **Test validation**: Unit test execution
- **Security scanning**: Secret detection

### Pre-merge Checks
- **All tests pass**: Unit, integration, and E2E tests
- **Code coverage**: Minimum 85% coverage maintained
- **Build success**: Production build completes successfully
- **Security validation**: No high-severity vulnerabilities

### Automated Workflows
- **CI/CD pipeline**: Automated testing and deployment
- **Dependency updates**: Automated security updates
- **Code quality**: SonarQube or similar analysis
- **Performance monitoring**: Bundle size and performance tracking

---

*This workflow ensures code quality, security, and maintainability*
*For development standards, see `/documentation/DEVELOPMENT_STANDARDS.md`*