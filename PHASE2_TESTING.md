# Phase 2: Access Rules & CV Management - Testing Documentation

## Overview
Phase 2 implements access rule enforcement, CV management enhancements, and recruiter shared pool functionality.

---

## Test Environment Setup

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB running
- Application built and running on \https://localhost:5001\

### Demo Accounts
\\\
Admin: admin@talentacquisition.local / Admin@123
Recruiter: recruiter@talentacquisition.local / Recruiter@123
Candidate: candidate@talentacquisition.local / Candidate@123
\\\

### Pre-seeded Data
- 3 demo user accounts with full profiles
- 4 positions with varying access rules
- 7 attribute definitions (English Level, GPA, Years of Experience, etc.)
- Sample projects and achievements

---

## Feature 1: Access Rule Enforcement on CV Listing

### Test Case 1.1: View CVs with Access Rules Applied
**Prerequisites:**
- Logged in as Recruiter
- Navigate to CVs listing page (/CVs)

**Steps:**
1. Observe the CV list filtered by access rules
2. Note the ViewBag message: "X application(s) hidden due to access rules not met"
3. Verify only eligible CVs are displayed

**Expected Result:**
- CVs shown only when candidate meets position's access rule criteria
- Admin message visible for hidden applications
- No broken references or 500 errors

### Test Case 1.2: Access Rule Filtering with Multiple Criteria
**Prerequisites:**
- Position has multiple access rules (e.g., GPA >= 3.0 AND Years of Experience >= 2)
- Candidates have varying attribute values

**Steps:**
1. As Recruiter, view CVs for that position
2. Observe filtering logic applies all rules

**Expected Result:**
- Only candidates meeting ALL rules shown
- Candidates missing any rule requirement filtered out
- Correct count of hidden applications displayed

---

## Feature 2: One CV Per Position Prevention

### Test Case 2.1: Attempt Duplicate Application Redirect
**Prerequisites:**
- Logged in as Candidate
- Previously applied to a position

**Steps:**
1. Navigate to position details
2. Click "Apply" button
3. Observe error message and redirect

**Expected Result:**
- Error message: "You already have an application for this position. You can update it instead."
- Redirected to Edit page for existing CV
- No duplicate CV created in database

---

## Feature 3: CV Deletion (Withdraw Application)

### Test Case 3.1: Candidate Deletes Own Application
**Prerequisites:**
- Logged in as Candidate
- Navigate to "My Applications" page

**Steps:**
1. Find an application
2. Click Delete/Withdraw button
3. Confirm deletion

**Expected Result:**
- Application removed from database
- Confirmation message: "Application deleted successfully"
- Redirected to My Applications
- Application no longer visible in list

---

## Feature 4: Recruiter Shared Pool Access

### Test Case 4.1: Recruiter Can Edit Positions Created by Other Recruiters
**Prerequisites:**
- Two recruiter accounts
- Position created by Recruiter A

**Steps:**
1. As Recruiter B, navigate to Positions
2. Find position created by Recruiter A
3. Edit the position

**Expected Result:**
- No "Access Denied" errors
- Recruiter B can modify position attributes
- Changes persist for all recruiters to see

---

## Full Integration Test

**Steps:**
1. **Recruiter creates position with access rules**
2. **Candidate completes profile and applies**
3. **Recruiter views CVs filtered by rules**
4. **Candidate deletes application**
5. **Recruiter sees updated list**

**Expected Result:**
- All steps complete successfully
- No authorization errors
- Data consistency maintained
- Access rules enforced throughout

---

## Regression Tests
- [ ] Candidates can create new CVs (first application)
- [ ] CV details page displays correctly
- [ ] Comments on CVs work correctly
- [ ] Tag cloud on homepage displays
- [ ] Profile editing works
- [ ] Each user sees only their own data

---

## Known Limitations / Future Work

1. **Admin User Management View:** Not yet implemented
   - Planned for Phase 4
   - Will include: Block/unblock users, edit profiles, manage roles

2. **Performance at Scale:** Not tested with 10,000+ CVs
   - Consider pagination for large datasets

---

Last Updated: 2026-07-22
Phase 2 Status: Code Complete, Testing Ready
