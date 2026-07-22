# Carer Onboarding - Compliance & Implementation Guide

## Overview
This document outlines the legal and compliance framework for the carer onboarding system for SEN (Special Educational Needs) students, with specific reference to UK GDPR, safeguarding legislation, and educational authority guidelines.

---

## 1. LEGAL & REGULATORY FRAMEWORK

### 1.1 Primary Legislation

#### UK General Data Protection Regulation (UK GDPR)
- **Regulation (EU) 2016/679 as retained in UK law**
- Key Articles relevant to this system:
  - **Article 6**: Lawful basis for processing (education provision & safeguarding)
  - **Article 7**: Conditions for consent (must be freely given, specific, informed)
  - **Article 13**: Information to be provided to data subjects
  - **Article 14**: Information where data not obtained from data subject
  - **Article 15**: Right of access / Subject Access Requests (SAR)
  - **Article 16**: Right to rectification
  - **Article 17**: Right to erasure ("right to be forgotten")
  - **Article 18**: Right to restrict processing
  - **Article 20**: Data portability rights
  - **Article 35**: Data Protection Impact Assessment (DPIA) requirement

#### Data Protection Act 2018
- Provides UK-specific exemptions and requirements
- Schedule 1 allows processing for education purposes
- Sensitive data processing for safeguarding purposes

#### Education Act 1996
- Statutory authority for schools to collect and process student data
- Duty of care to students
- Requirement to keep student records including parental information

#### Children Act 1989 & 2004
- **Section 17**: Duty to safeguard and promote welfare of children
- Parental responsibility definitions (s3)
- Allows processing of parental data for child welfare
- Overrides some GDPR restrictions when child protection at stake

#### Equality Act 2010
- Requirement to make reasonable adjustments for disabled students (including SEN)
- Data about disabilities/medical conditions for accessibility purposes
- Non-discrimination obligations

#### Safeguarding & Child Protection
- **DfE (Department for Education) statutory guidance**
- **KCSIE (Keeping Children Safe in Education)**
- Local Safeguarding Children Partnership (LSCP) requirements
- Information sharing protocols between agency

---

## 2. LAWFUL BASIS FOR PROCESSING

### 2.1 Role of Lawful Basis
Before processing any personal data, organizations must establish one or more lawful bases under Article 6 of UK GDPR.

### 2.2 Applicable Lawful Bases

| Basis | Application | Example |
|-------|-------------|---------|
| **Article 6(1)(a) - Consent** | Freely given, specific, informed consent | Consent for daily progress updates, photo consent |
| **Article 6(1)(c) - Legal Obligation** | Required by law | Keeping education records (Education Act 1996) |
| **Article 6(1)(e) - Public Task** | Necessary for official functions | Providing education and safeguarding students |
| **Article 10 - Special Category** | Processing data about vulnerable persons | Medical data, SEN information (processing for safeguarding/education permitted) |

### 2.3 Implementation Approach
This onboarding system uses a **multi-basis approach**:
- **Legal Obligation** (primary): Education record keeping
- **Public Task** (primary): Student safeguarding and education provision
- **Consent** (secondary): Communication preferences, photos/videos

**Important**: Consent is NOT the basis for core education provision. The school can process essential safeguarding/medical data under legal obligation and public task. However, consent is required for non-essential uses (daily updates preference, photos).

---

## 3. SPECIAL CATEGORY DATA HANDLING

Special category data (sensitive data) requires higher protections.

### 3.1 Categories Collected
- **Health data**: Medical conditions, allergies, medication
- **Disability data**: SEN status, EHCP, accessibility requirements
- **Potentially safeguarding data**: Information suggesting child in need

### 3.2 Legal Basis for Special Category Processing
**Article 9(2)(h) - Protection of Vital Interests**: When processing is necessary for safeguarding purposes with proper safeguards.

### 3.3 Implementation Requirements
- ✅ Separate, encrypted storage
- ✅ Restricted access (need-to-know basis)
- ✅ Data Processing Record maintained
- ✅ Staff training on confidentiality
- ✅ Audit trail of access
- ✅ Clear retention periods
- ✅ Data subject informed of processing (Privacy Notice)

---

## 4. DATA PROTECTION IMPACT ASSESSMENT (DPIA)

For a new collection system handling children's data, a DPIA is **required** under Article 35 UK GDPR.

### 4.1 Key DPIA Elements
- Identification of processing risks
- Mitigation measures
- Necessity and proportionality assessment
- Children's rights assessment
- Approval documentation

### 4.2 Specific Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| Unauthorized access to medical data | Encryption, role-based access, audit logs |
| Loss of data | Secure backup, disaster recovery plan |
| Misidentification of parental responsibility | Verification process, legal declaration |
| Safeguarding data misuse | Staff training, access restrictions, monitoring |
| Retention beyond legal necessity | Automated deletion, retention schedule |
| Child sees restricted information | Access controls by role, data minimization |

### 4.3 AI/Automated Decision-Making
If implementing any automated decisions (e.g., auto-approval based on criteria), this increases DPIA requirements. Currently, manual approval is recommended.

---

## 5. PARENTAL RESPONSIBILITY VERIFICATION

### 5.1 Legal Context
Under Children Act 1989, only persons with **parental responsibility** can exercise consent on behalf of a child.

### 5.2 Who Has Parental Responsibility?
- ✓ Biological mother (automatically)
- ✓ Biological father (if married to mother OR on birth certificate after Dec 2003 OR court order)
- ✓ Legal guardian appointed by court
- ✓ Person with special guardianship order
- ✗ Stepparents (unless formally adopted)
- ✗ Grandparents (unless legal guardian)

### 5.3 Verification Process
The form includes a statutory declaration. To strengthen this:

**Recommended Verification Steps:**
1. **Initial Declaration** (completed) - Self-certification with penalty clause
2. **Identity Verification** (optional add-on)
   - Request government-issued ID (driving license, passport)
   - Verify photo against user
   - Store in secure document vault
3. **Court Order Check** (for high-needs students)
   - Contact local family court or children's services for known restrictions
   - Document review in file

**Documentation Retention:**
- Declaration signed & dated (original or digital signature)
- Verification documents in separate secure file
- Audit trail of who verified and when

---

## 6. CONSENT MANAGEMENT

### 6.1 Consent vs. Legitimate Interest
**Key Distinction:**
- **Consent**: Freely given choice; can be withdrawn anytime
- **Legitimate Interest**: Necessary for core operations; more stable

**Application in Onboarding:**
- ✅ Core education/safeguarding: Legitimate interest + legal obligation (NOT dependent on consent)
- ✅ Daily updates communications: Consent (can opt-in/out)
- ✅ Photography/video: Explicit consent (separate opt-in)
- ✅ Sharing with third parties: Explicit consent (separate opt-in)

### 6.2 Consent Recording
- **Record**: Yes/No, date/time, IP address, method (online form/verbal)
- **Audit Trail**: Maintain history of all changes
- **Withdrawal**: Simple process, recorded similarly

### 6.3 Refresh Requirements
- Initial consent valid for: enrollment period
- Re-consent required if: material change in processing, 2+ years elapsed (varies by consent type)

---

## 7. PRIVACY NOTICE REQUIREMENTS (ARTICLE 13/14)

### 7.1 Required Content
The embedded privacy notice in the form covers:
- ✅ Identity of controller
- ✅ Purpose of processing
- ✅ Lawful basis
- ✅ Recipients
- ✅ Retention period
- ✅ Data subject rights
- ✅ Right to withdraw consent
- ✅ Right to complain to ICO

### 7.2 Plain Language
The form uses Plain English principles (recommended Flesch-Kincaid Grade 8-10).

### 7.3 Records Provided to Data Subject
- ✅ Privacy notice (in form)
- ✅ Copy of their submitted data
- ✅ Data retention schedule
- ✅ How to exercise rights

---

## 8. DATA RETENTION & DELETION

### 8.1 Retention Schedule

| Data Category | Retention Period | Basis |
|---------------|-----------------|-------|
| Core enrollment data | Until end of enrollment + 6 years | Educational records requirement |
| Safeguarding notes | Until 25th birthday of child + 6 years | Children's safeguarding best practice |
| Medical/dietary info | Duration + 3 years | Medical records guidance |
| Communications/consent records | Duration + 2 years | GDPR accountability principle |
| Access/audit logs | 2 years | Security & compliance |

### 8.2 Deletion Process
1. **Flagged for deletion** based on retention schedule
2. **Anonymization considered** if research/reporting possible without identification
3. **Secure deletion** from primary system (secure wipe, not just deletion)
4. **Backup purge** after backup retention expires
5. **Deletion recorded** in audit log
6. **Confirmation** provided if subject requests

### 8.3 Exceptions (Data Retained Beyond Schedule)
- Active court proceedings or investigations
- Safeguarding concern documented
- Data subject requests retention
- Legal hold order in place

---

## 9. CHILDREN'S DATA RIGHTS & PROTECTIONS

### 9.1 Age of Data Subject
- **Under 13**: Parental consent required for direct contact communication
- **13-18**: May exercise own rights in limited circumstances (depends on school policy)
- **17+**: Often treated as young adult (consult Data Protection guidance)

### 9.2 Application in Onboarding
- **Parents/guardians** complete onboarding and consent on behalf of child
- **Child's data rights**: Can access own record (via parent typically)
- **Safeguarding override**: If child's welfare at risk, consent/age may be overridden

### 9.3 Special Protections
- Data minimization - only collect what's necessary
- Transparent communication about processing
- Clear explanation of parental role
- Child's voice considered in safeguarding decisions

---

## 10. SUBJECT ACCESS REQUESTS (DATA SUBJECT RIGHTS)

### 10.1 Key Rights
Under Article 15 et seq., carers can request:
- **Access** to their own data
- **Rectification** of incorrect data
- **Erasure** (limited applicability; can be refused for school record purposes)
- **Restriction** of processing
- **Data Portability** (limited applicability for children's records)
- **Object** to processing (limited in education context)

### 10.2 Response Timeline
- **30 calendar days** from receipt of request
- Extensions possible for complex requests (+60 days)
- No fee charged for access requests

### 10.3 Implementation Process
1. **Request Received** - record date, identity verify, document request
2. **Scope Determined** - what data is relevant?
3. **Collation** - gather from systems (email, file store, CMS, etc.)
4. **Redaction** (if applicable) - remove third-party data requiring consent
5. **Format** - typically PDF/Excel for digital data
6. **Response** - provide secure transmission method
7. **Record** - log request in compliance register

### 10.4 Refusals
Can refuse if:
- Request manifestly unfounded or excessive
- Educational record kept solely for child's benefit
- Data held for educational/examination assessment

---

## 11. SAFEGUARDING & INFORMATION SHARING

### 11.1 Legal Duty to Share
Information sharing for safeguarding **overrides GDPR** when:
- Child believed to be at risk
- Public protection concerned
- Crime suspected
- Court order in place

### 11.2 Entities That May Receive Data Without Consent
- **Children's Social Care / Local Authority**
- **Police**
- **Health professionals** (GP, school nurse)
- **National Safeguarding Partners**
- **Multi-Agency Safeguarding Hub (MASH)**

### 11.3 Documentation
- Record: What information shared, to whom, when, why
- Review: Was sharing necessary and proportionate?
- Communication: When appropriate, inform data subject

### 11.4 SEN-Specific Considerations
- Additional vulnerabilities of SEN students
- Communication/consent capacity considerations
- Multi-agency involvement (health, education, social care)
- EHCP (Education, Health & Care Plan) processes

---

## 12. SYSTEM SECURITY REQUIREMENTS

### 12.1 Technical Safeguards

**Encryption:**
- ✅ In-transit: TLS 1.2+
- ✅ At-rest: AES-256 for sensitive fields (medical, safeguarding data)
- ✅ Backups: Encrypted, separate from production

**Access Control:**
- ✅ Role-based access control (RBAC)
- ✅ Multi-factor authentication for sensitive functions
- ✅ Session timeout (30 minutes inactivity)
- ✅ Least privilege principle

**Audit & Monitoring:**
- ✅ All access logged (who, what, when)
- ✅ Attempt failures logged
- ✅ Alerts for suspicious activity
- ✅ Monthly access reviews

### 12.2 Organizational Safeguards
- ✅ Data Protection Officer (DPO) or designated person
- ✅ Staff training (GDPR + safeguarding annually)
- ✅ Privacy by design principles
- ✅ Incident response plan
- ✅ Vendor/processor agreements in place

### 12.3 Breach Response
If personal data breach occurs:
1. **Immediate**: Contain breach, inventory affected records
2. **Assessment**: Risk to individuals? (likely = notification required)
3. **Notification**: 
   - DPO/leadership within 1-2 days
   - ICO notification if high risk (not excessive delay)
   - Data subject notification if high risk to their rights
4. **Documentation**: Maintain breach record (3-year retention)

---

## 13. DATA PROCESSING AGREEMENT (with external processors)

If using external services (e.g., cloud storage, email), a **Data Processing Agreement (DPA)** is required under **Article 28 UK GDPR**.

### 13.1 Essential DPA Clauses
- Processor acts only on instruction
- Confidentiality obligations on processor staff
- Sub-processor management
- International transfer restrictions
- Assistance with data subject rights requests
- Assistance with obligations (DPIA, breach notification)
- Deletion/return of data at end of contract
- Audit rights

### 13.2 Cloud Storage Example
If storing in Azure/Google Cloud:
- Verify DPA provided
- Ensure encryption enabled
- Restrict data center location (UK-based preferred)
- Review processor incident procedures

---

## 14. IMPLEMENTATION CHECKLIST

### Pre-Launch
- [ ] DPIA completed and reviewed
- [ ] Privacy notice drafted and approved
- [ ] Staff training completed (GDPR + safeguarding)
- [ ] Retention policy documented
- [ ] Technical security controls implemented
- [ ] Data Processing Agreement with vendors signed
- [ ] Incident response plan documented
- [ ] Safeguarding lead sign-off obtained

### During Operation
- [ ] Consent records maintained with audit trail
- [ ] Access logs reviewed monthly
- [ ] Deletion scheduled per retention policy
- [ ] Subject Access Requests processed within 30 days
- [ ] Staff training refreshed annually
- [ ] Breach log maintained

### Ongoing
- [ ] Parental rights information provided in literature
- [ ] Contact information for privacy queries displayed
- [ ] Technology/legislation changes monitored
- [ ] System audit conducted annually

---

## 15. USEFUL RESOURCES

### Official Guidance
- **ICO (Information Commissioner's Office)**: https://ico.org.uk
  - GDPR information for schools
  - Best practice guidance
- **DfE (Department for Education)**: https://www.gov.uk/dfe
  - Education data handling standards
  - Safeguarding guidance
- **Information Standards Board**: https://www.official-documents.co.uk

### Relevant Legislation
- UK GDPR: https://www.legislation.gov.uk/eur/2016/679
- Data Protection Act 2018: https://www.legislation.gov.uk/ukpga/2018/12
- Children Act 1989: https://www.legislation.gov.uk/ukpga/1989/41
- Education Act 1996: https://www.legislation.gov.uk/ukpga/1996/56

### Training & Certification
- ICO GDPR training
- Safeguarding training providers (local authority)
- Data Protection Officer network

---

## 16. APPROVAL & SIGN-OFF

This onboarding system should be reviewed and approved by:
- [ ] **Headteacher/Principal**: Educational governance
- [ ] **School Business Manager**: Operational compliance
- [ ] **Safeguarding Lead**: Child protection alignment
- [ ] **Data Protection Officer** (or external reviewer): GDPR compliance
- [ ] **Governors** (if appropriate): School policy approval

---

**Document Version:** 1.0  
**Last Updated:** [Today's Date]  
**Review Date:** [Date + 12 months]  
**Approved By:** [Name/Title]
