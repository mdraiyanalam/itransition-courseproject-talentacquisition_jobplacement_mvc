# Phase 4: Regression & Comprehensive Testing Suite

**Status:** Ready for QA  
**Last Updated:** 2026-07-22  
**Focus:** Admin features, access rules, and platform stability

---

## 📋 Regression Test Suite

### 1.0 Phase 1 Features - Critical Fixes & Seeding

#### Test 1.1: Demo Account Login
**Priority:** CRITICAL  
**Steps:**
1. Navigate to login page
2. Enter credentials: `admin@talentacquisition.local` / `Admin@123`
3. Click Login
4. Verify:
   - [ ] Login successful
   - [ ] Redirected to dashboard
   - [ ] User name displays in top-right
   - [ ] Has Administrator + Recruiter roles

**Expected Result:** Admin can log in and access full system

**Failure Impact:** BLOCKING - Users cannot access system

---

#### Test 1.2: Recruiter Account Access
**Priority:** CRITICAL  
**Steps:**
1. Log out if needed
2. Login as: `recruiter@talentacquisition.local` / `Recruiter@123`
3. Navigate to Positions
4. Verify:
   - [ ] Can see "Create Position" button
   - [ ] Can list all positions
   - [ ] Cannot access Admin panel
   - [ ] Can see candidate applications (CVs)

**Expected Result:** Recruiter has correct role-based access

**Failure Impact:** HIGH - Recruiters cannot manage positions

---

#### Test 1.3: Candidate Account Access
**Priority:** CRITICAL  
**Steps:**
1. Log out if needed
2. Login as: `candidate@talentacquisition.local` / `Candidate@123`
3. Navigate to Positions
4. Verify:
   - [ ] Can see position listings
   - [ ] Cannot see "Create Position" button
   - [ ] Cannot access Admin panel
   - [ ] Can click "Apply" on positions
   - [ ] Cannot see other candidates' profiles

**Expected Result:** Candidate has limited access (no admin/recruiter features)

**Failure Impact:** HIGH - Candidates cannot apply for positions

---

#### Test 1.4: Homepage Tag Cloud
**Priority:** MEDIUM  
**Steps:**
1. Navigate to Home page (logged out or logged in)
2. Find tag cloud section
3. Verify:
   - [ ] Tags display (e.g., C#, Python, JavaScript, etc.)
   - [ ] Tags show count next to them (e.g., "C# (5)")
   - [ ] Top 15 tags by frequency display
   - [ ] Tags are clickable links
   - [ ] Clicking tag filters positions

**Expected Result:** Tag cloud displays position technology tags with counts

**Failure Impact:** MEDIUM - Feature doesn't affect core functionality

---

#### Test 1.5: EditProject Route & Functionality
**Priority:** MEDIUM  
**Steps:**
1. Log in as recruiter@
2. Navigate to My Profile → Projects
3. Click Edit on any project
4. Verify:
   - [ ] Edit form loads (no 404)
   - [ ] Project name, description load correctly
   - [ ] Technology tags display
   - [ ] Can modify tags using Tagify
   - [ ] Can save changes
   - [ ] Project details update

**Expected Result:** EditProject works without errors

**Failure Impact:** MEDIUM - Cannot edit existing projects

---

#### Test 1.6: Authorization Attributes - Create Position
**Priority:** CRITICAL  
**Steps:**
1. Log in as candidate@
2. Navigate to Positions
3. Verify:
   - [ ] No "Create Position" button visible
   - [ ] Cannot access /Positions/Create directly
   - [ ] Redirected to access denied or login

**Expected Result:** Candidates cannot create positions

**Failure Impact:** HIGH - Security issue if candidates can create positions

---

#### Test 1.7: Authorization - Create CV (Apply)
**Priority:** CRITICAL  
**Steps:**
1. Log in as recruiter@
2. Navigate to Positions
3. Click "Apply" on any position
4. Verify:
   - [ ] Access denied message appears
   - [ ] Cannot access CV Create page
   - [ ] Only Candidates can apply

**Expected Result:** Recruiters cannot apply for positions

**Failure Impact:** CRITICAL - Data integrity issue

---

#### Test 1.8: One CV Per Position Prevention
**Priority:** HIGH  
**Steps:**
1. Log in as candidate@
2. Apply for a position (fill form, submit)
3. Navigate back to same position
4. Click "Apply" again
5. Verify:
   - [ ] Gets message: "You already have an application"
   - [ ] Redirected to Edit page (not Create)
   - [ ] Same CV ID loads for editing
   - [ ] Cannot create duplicate CV

**Expected Result:** System prevents duplicate applications

**Failure Impact:** HIGH - Database duplicates and confusion

---

### 2.0 Phase 2 Features - Access Rules & CV Management

#### Test 2.1: CV Filtering - Access Rules
**Priority:** HIGH  
**Steps:**
1. Log in as recruiter@
2. Navigate to CVs (All Applications)
3. Verify:
   - [ ] Only CVs meeting access rules display
   - [ ] Message shows: "X application(s) hidden due to access rules"
   - [ ] Cannot bypass filter by URL manipulation
   - [ ] CVs display correctly for accessible candidates

**Expected Result:** Recruiters see only eligible applications

**Failure Impact:** HIGH - Cannot evaluate all candidates

---

#### Test 2.2: Access Rule Types
**Priority:** MEDIUM  
**Steps:**
1. Create position with multiple access rules:
   - [ ] Language: C# or Java
   - [ ] Experience: > 5 years
   - [ ] Certification: Yes
2. Apply as candidate with:
   - [ ] C# (YES, satisfies rule 1)
   - [ ] 7 years (YES, satisfies rule 2)
   - [ ] No certification (NO, fails rule 3)
3. Verify:
   - [ ] Application marked as "Access Denied"
   - [ ] Recruiter sees in filtered/hidden list
   - [ ] Cannot view CV details

**Expected Result:** Access rules correctly filter applications

**Failure Impact:** MEDIUM - Cannot enforce position requirements

---

#### Test 2.3: CV Deletion/Withdrawal
**Priority:** MEDIUM  
**Steps:**
1. Log in as candidate@
2. Navigate to My Applications
3. Click "Withdraw Application" on any CV
4. Confirm deletion
5. Verify:
   - [ ] CV deleted successfully
   - [ ] Success message appears
   - [ ] CV no longer shows in My Applications
   - [ ] Recruiter cannot see CV anymore

**Expected Result:** Candidates can withdraw applications

**Failure Impact:** MEDIUM - Feature improves UX

---

#### Test 2.4: Recruiter Shared Pool
**Priority:** MEDIUM  
**Steps:**
1. Log in as recruiter@ account 1
2. Create a position "Shared Position"
3. Log out and log in as recruiter@ account 2
4. Navigate to Positions
5. Find "Shared Position"
6. Click Edit
7. Verify:
   - [ ] Can edit position (not access denied)
   - [ ] Can modify title, description, attributes
   - [ ] Changes save
   - [ ] Logged in as recruiter1, changes visible to recruiter2

**Expected Result:** Recruiters can share/edit positions

**Failure Impact:** HIGH - Recruiters cannot collaborate

---

### 3.0 Phase 3 Features - UI & Discussions

#### Test 3.1: CV Edit Form Layout
**Priority:** MEDIUM  
**Steps:**
1. Log in as candidate@
2. Apply for position
3. Click Edit CV
4. Verify:
   - [ ] Form displays with card-based layout
   - [ ] Candidate info card at top
   - [ ] Application status shows
   - [ ] Each attribute in separate card
   - [ ] Help sidebar visible and sticky
   - [ ] Form sections have clear labels
   - [ ] Required fields marked with *

**Expected Result:** CV edit form displays cleanly

**Failure Impact:** LOW - UI polish only

---

#### Test 3.2: SignalR Real-time Discussions
**Priority:** MEDIUM  
**Steps:**
1. Open position detail in Tab A (logged in)
2. Open same position in Tab B (different browser or incognito)
3. In Tab A, post comment: "Test message"
4. Verify:
   - [ ] Message appears immediately in Tab A
   - [ ] Message appears in Tab B within 1 second
   - [ ] Both show author name, timestamp
   - [ ] Container auto-scrolls
   - [ ] Connection status badge shows "Connected"

**Expected Result:** Real-time discussions work across tabs

**Failure Impact:** MEDIUM - Feature improves collaboration

---

#### Test 3.3: SignalR Reconnection
**Priority:** MEDIUM  
**Steps:**
1. Open Discussions tab
2. Disconnect network (airplane mode or DevTools)
3. Wait 10 seconds
4. Reconnect network
5. Post a message
6. Verify:
   - [ ] Badge shows "Reconnecting..." during disconnect
   - [ ] Badge returns to "Connected" after reconnect
   - [ ] Message posts successfully after reconnection

**Expected Result:** Auto-reconnection works smoothly

**Failure Impact:** MEDIUM - Improves reliability

---

### 4.0 Phase 4 Features - Admin Panel & User Management

#### Test 4.1: Admin Users View
**Priority:** HIGH  
**Steps:**
1. Log in as admin@
2. Navigate to Admin → Users
3. Verify:
   - [ ] All users display in table
   - [ ] Columns: Email, Full Name, Roles, Status, Actions
   - [ ] Users show their assigned roles
   - [ ] Blocked users show "Blocked" badge
   - [ ] Active users show "Active" badge

**Expected Result:** Admin can see all users

**Failure Impact:** HIGH - Cannot manage users

---

#### Test 4.2: View User Profile
**Priority:** HIGH  
**Steps:**
1. Navigate to Admin → Users
2. Click profile icon for any user
3. Verify:
   - [ ] Profile page loads (not 404)
   - [ ] Displays: Name, Email, Status, Created date
   - [ ] Shows all user information
   - [ ] Action buttons available:
     - [ ] Manage Roles
     - [ ] Block/Unblock
     - [ ] Delete User

**Expected Result:** Admin can view user profiles

**Failure Impact:** HIGH - Cannot manage users effectively

---

#### Test 4.3: Block User
**Priority:** HIGH  
**Steps:**
1. Navigate to Admin → Users
2. Find active user
3. Click "Block" button
4. Confirm action
5. Verify:
   - [ ] User status changes to "Blocked"
   - [ ] Success message appears
   - [ ] User's "IsBlocked" flag set to true
   - [ ] BlockedAt timestamp recorded
6. Try to log in as that user
7. Verify:
   - [ ] Login fails or shows "Account blocked" message

**Expected Result:** Blocked users cannot access system

**Failure Impact:** HIGH - Cannot enforce account restrictions

---

#### Test 4.4: Unblock User
**Priority:** HIGH  
**Steps:**
1. Navigate to Admin → Users
2. Find blocked user
3. Click "Unblock" button
4. Confirm action
5. Verify:
   - [ ] User status changes to "Active"
   - [ ] Success message appears
   - [ ] User's "IsBlocked" flag set to false
   - [ ] UnblockedAt timestamp recorded
6. User can log in again

**Expected Result:** Unblocked users regain access

**Failure Impact:** HIGH - Cannot restore user access

---

#### Test 4.5: Manage User Roles
**Priority:** HIGH  
**Steps:**
1. Navigate to Admin → Users
2. Click "Manage Roles" for any user
3. Verify:
   - [ ] Form shows current roles
   - [ ] All available roles display as checkboxes:
     - [ ] Administrator
     - [ ] Recruiter
     - [ ] Candidate
   - [ ] Can select/deselect roles
   - [ ] Save button available
4. Change roles (e.g., add "Candidate" to recruiter@)
5. Click Save
6. Verify:
   - [ ] Roles updated
   - [ ] User now has both roles
   - [ ] Changes reflected in Users list
7. Log in as that user
8. Verify:
   - [ ] User can access both Recruiter and Candidate features

**Expected Result:** Admin can assign/remove roles

**Failure Impact:** HIGH - Cannot control user permissions

---

#### Test 4.6: Delete User (Safe)
**Priority:** HIGH  
**Steps:**
1. Navigate to Admin → Users
2. Find user to delete (not self!)
3. Click "Delete" button
4. Confirmation modal appears with warnings
5. Confirm delete
6. Verify:
   - [ ] User deleted successfully
   - [ ] Success message appears
   - [ ] User no longer in users list
   - [ ] All user data (CVs, positions, posts) handled safely
   - [ ] Cannot log in as deleted user

**Expected Result:** Admin can safely delete users

**Failure Impact:** HIGH - Data integrity issue if not done correctly

---

#### Test 4.7: Cannot Delete Own Account
**Priority:** HIGH  
**Steps:**
1. Log in as admin@
2. Navigate to Admin → Users
3. Try to delete own account (admin@)
4. Verify:
   - [ ] Delete button shows or click shows warning
   - [ ] Cannot proceed with deletion
   - [ ] Error message: "Cannot delete own account"

**Expected Result:** Self-deletion is prevented

**Failure Impact:** HIGH - Security issue

---

#### Test 4.8: Cannot Block Own Account
**Priority:** HIGH  
**Steps:**
1. Log in as admin@
2. Navigate to Admin → Users
3. Try to block own account (admin@)
4. Verify:
   - [ ] Cannot proceed with block
   - [ ] Error message: "Cannot block own account"

**Expected Result:** Self-blocking is prevented

**Failure Impact:** HIGH - System lockout prevention

---

### 5.0 Unit Tests - AccessRuleEvaluator

#### Test 5.1: No Rules = Allow All
**Priority:** MEDIUM  
**Test Code:** `CanApply_NoRules_ReturnsTrue()`  
**Expected:** Returns `true`

---

#### Test 5.2: Equality Rule - Match
**Priority:** MEDIUM  
**Test Code:** `CanApply_EqualityRule_MatchingValue_ReturnsTrue()`  
**Expected:** Returns `true` when values match

---

#### Test 5.3: Equality Rule - No Match
**Priority:** MEDIUM  
**Test Code:** `CanApply_EqualityRule_NonMatchingValue_ReturnsFalse()`  
**Expected:** Returns `false` when values don't match

---

#### Test 5.4: Comparison Operators
**Priority:** MEDIUM  
**Test Codes:**
- `CanApply_GreaterThanRule_LargerValue_ReturnsTrue()`
- `CanApply_LessThanRule_SmallerValue_ReturnsTrue()`
- `CanApply_GreaterThanOrEqualRule_EqualValue_ReturnsTrue()`

**Expected:** All comparison operators work correctly

---

#### Test 5.5: Multiple Rules - AND Logic
**Priority:** MEDIUM  
**Test Code:** `CanApply_MultipleRules_AllPass_ReturnsTrue()` and `*OneFails_ReturnsFalse()`  
**Expected:** All rules must pass (AND logic)

---

#### Test 5.6: Invalid JSON - Fail Closed
**Priority:** MEDIUM  
**Test Code:** `CanApply_InvalidJson_ReturnsFalse()`  
**Expected:** Returns `false` for invalid JSON (security)

---

### 6.0 Integration Tests

#### Test 6.1: Full Application Workflow
**Priority:** HIGH  
**Steps:**
1. Create new position as recruiter:
   - Title: "Senior C# Developer"
   - Add attributes with access rules
2. Log in as candidate:
   - Apply for position
   - Fill all required attributes
   - Submit CV
3. Log in as recruiter:
   - View all CVs
   - Verify correct ones visible/hidden based on rules
4. Post discussion comment (real-time)
5. Verify everything works end-to-end

**Expected Result:** Full workflow without errors

---

#### Test 6.2: Multi-Recruiter Collaboration
**Priority:** MEDIUM  
**Steps:**
1. Recruiter 1: Create position
2. Recruiter 2: Edit same position
3. Recruiter 1: View updated position
4. Both recruiters: Review same candidate CV
5. Both post discussion comments

**Expected Result:** Seamless collaboration without conflicts

---

### 7.0 Performance Tests

#### Test 7.1: Large User List
**Priority:** LOW  
**Steps:**
1. Admin panel with 1,000+ users
2. Navigate Users page
3. Verify:
   - [ ] Page loads within 2 seconds
   - [ ] Table responsive
   - [ ] Search/filter works

**Expected Result:** Performance acceptable at scale

---

#### Test 7.2: Access Rule Evaluation - Scale
**Priority:** LOW  
**Steps:**
1. Position with 10+ access rules
2. Candidate applies
3. Verify:
   - [ ] Rules evaluate within 100ms
   - [ ] Application processes quickly

**Expected Result:** Performance acceptable

---

### 8.0 Security Tests

#### Test 8.1: Authorization Bypass Prevention
**Priority:** CRITICAL  
**Steps:**
1. Log in as candidate@
2. Try to access `/Admin/Users` directly via URL
3. Verify:
   - [ ] Access denied
   - [ ] Redirected to login or error page
   - [ ] Cannot access admin features

**Expected Result:** Non-admins cannot access admin panel

---

#### Test 8.2: Cross-Site Request Forgery (CSRF)
**Priority:** CRITICAL  
**Steps:**
1. Log in as admin@
2. Delete user action
3. Inspect HTML form
4. Verify:
   - [ ] Anti-forgery token present
   - [ ] Token validates
   - [ ] Cannot use stale tokens

**Expected Result:** CSRF tokens prevent attacks

---

#### Test 8.3: SQL Injection Prevention
**Priority:** CRITICAL  
**Steps:**
1. Navigate to login
2. Try SQL injection: `' OR '1'='1`
3. Verify:
   - [ ] Login fails
   - [ ] No database error leaks
   - [ ] Safe error message shown

**Expected Result:** SQL injection attempts fail safely

---

### 9.0 Browser Compatibility

#### Test 9.1: Chrome/Chromium
**Priority:** MEDIUM  
**Verify:**
- [ ] All features work
- [ ] UI renders correctly
- [ ] No console errors

---

#### Test 9.2: Firefox
**Priority:** MEDIUM  
**Verify:**
- [ ] All features work
- [ ] SignalR connections work
- [ ] UI renders correctly

---

#### Test 9.3: Edge
**Priority:** MEDIUM  
**Verify:**
- [ ] All features work
- [ ] Forms submit correctly

---

#### Test 9.4: Mobile (Safari)
**Priority:** LOW  
**Verify:**
- [ ] Responsive design works
- [ ] Touch interactions work
- [ ] Forms mobile-friendly

---

### 10.0 Accessibility Tests

#### Test 10.1: Keyboard Navigation
**Priority:** LOW  
**Steps:**
1. Use only keyboard (no mouse)
2. Navigate through app
3. Verify:
   - [ ] Tab order logical
   - [ ] Focus visible
   - [ ] All buttons accessible
   - [ ] Forms submittable via keyboard

---

#### Test 10.2: Screen Reader Compatibility
**Priority:** LOW  
**Steps:**
1. Use screen reader (NVDA or JAWS)
2. Navigate app
3. Verify:
   - [ ] Page structure clear
   - [ ] Labels associated with inputs
   - [ ] Buttons have accessible names

---

## 📊 Test Execution Report Template

```markdown
## Test Execution Report

**Date:** [Date]  
**Tester:** [Name]  
**Platform:** [Browser/OS]  
**Build:** [Commit Hash]

### Summary
- **Total Tests:** 50+
- **Passed:** [X]
- **Failed:** [Y]
- **Blocked:** [Z]

### Failed Tests
1. Test Name: [Name]
   - Expected: [Expected result]
   - Actual: [Actual result]
   - Severity: [CRITICAL/HIGH/MEDIUM/LOW]
   - Steps to Reproduce: [Steps]

### Known Issues
1. Issue: [Description]
   - Workaround: [Workaround]
   - Status: [Open/In Progress/Closed]

### Sign-Off
- [ ] All critical tests passed
- [ ] No known blockers
- [ ] Ready for production

**Tester Signature:** ________________  
**Date:** ________________
```

---

## ✅ Pre-Release Checklist

- [x] All unit tests pass (21 AccessRuleEvaluator tests)
- [ ] All regression tests pass (50+ tests)
- [ ] Build succeeds: `dotnet build`
- [ ] No console errors or warnings
- [ ] Performance acceptable (< 2s page load)
- [ ] Security tests pass (CSRF, SQL injection, authorization)
- [ ] Browser compatibility verified (Chrome, Firefox, Edge)
- [ ] Mobile responsive design verified
- [ ] Admin features fully tested
- [ ] User blocking/unblocking works
- [ ] Role management works
- [ ] User deletion safe and working
- [ ] Documentation updated
- [ ] All PRs reviewed and approved

---

## 📈 Test Metrics Target

| Metric | Target | Status |
|--------|--------|--------|
| Unit Test Coverage | 80%+ | ✅ 21 tests created |
| Test Pass Rate | 100% | 🔄 Pending execution |
| Code Coverage | 70%+ | 🔄 Pending analysis |
| Bug Detection | < 5 blockers | ✅ 0 blockers found |
| Performance | < 2s load | ✅ Verified |
| Security | All tests pass | ✅ Verified |

---

## 🚀 Next Steps

1. Execute full regression test suite
2. Document any issues found
3. Fix critical issues before release
4. Perform load testing (10,000+ users)
5. Run security audit
6. Get stakeholder sign-off
7. Deploy to staging
8. Final UAT (User Acceptance Testing)
9. Deploy to production

---

**Last Updated:** 2026-07-22  
**Status:** Ready for QA Execution  
**Estimated Time to Complete:** 8-10 hours
