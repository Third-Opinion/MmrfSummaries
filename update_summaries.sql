-- ================================================================
-- SQL UPDATE Statements for nlp_extensions Database
-- ================================================================
-- Generated: 2025-07-31 00:54:10
-- Source: out.csv
-- Records: 2
-- ================================================================

-- Execute these statements to update the nlp_extensions table
-- with AI-generated summaries from the clinical trial data.

UPDATE nlp_extensions
SET summary = 'The MMRC Horizon One Adaptive Platform Trial (NCT06171685) is an innovative clinical study designed to efficiently evaluate optimal therapies for patients with relapsed or refractory multiple myeloma (RRMM). This adaptive platform trial employs a flexible Master Protocol structure that allows multiple investigational treatment arms to be added or modified over time as new therapies become trial-ready and as patient accrual supports additional arms. The trial''s adaptive design enables it to evolve continuously, with individual arms terminating according to their specific protocols, addressing the critical research challenge of determining the most effective personalized treatment approaches for RRMM patients.

The study targets adults aged 18-99 years of all genders who have relapsed or refractory multiple myeloma, with current interventions focusing on teclistamab, both as monotherapy and in combination regimens. The platform''s structure is specifically designed to maximize efficiency in therapeutic evaluation by allowing simultaneous testing of multiple treatment strategies under a unified protocol framework. This approach represents a significant advancement in clinical trial methodology for multiple myeloma, as it can rapidly incorporate emerging therapies and adapt to new scientific developments while maintaining rigorous clinical trial standards, ultimately aiming to accelerate the identification of optimal treatment sequences and combinations for this challenging hematologic malignancy.', short_summary = 'This is an adaptive platform trial for relapsed/refractory multiple myeloma that uses a Master Protocol structure allowing multiple investigational treatment arms (including teclistamab) to be added or removed over time based on enrollment and readiness. The trial''s flexible design aims to efficiently evaluate the best therapies for individual patients with this condition.'
WHERE nct_id = 'NCT06171685';

UPDATE nlp_extensions
SET summary = 'This Phase 3 randomized controlled trial (NCT06158841) is evaluating the efficacy of etentamig, an investigational intravenous monotherapy, compared to standard available therapies in adults with relapsed or refractory multiple myeloma (R/R MM). Multiple myeloma is a blood cancer affecting plasma cells that commonly causes bone pain, fractures, infections, and kidney complications, with current treatments often failing due to disease relapse or resistance. The study aims to enroll approximately 380 adult participants across 140 global sites, randomizing them into two arms: Arm A receiving etentamig monotherapy and Arm B receiving standard available therapy (SAT) as determined by investigators according to local guidelines and approved treatments.

The methodology involves a 3.5-year study duration with 28-day treatment cycles, where participants in Arm A receive intravenous etentamig infusions while those in Arm B receive investigator-selected standard therapies including carfilzomib, pomalidomide, elotuzumab, selinexor, bortezomib, or dexamethasone combinations. The target population consists of adults (18+ years) of all genders with R/R MM who have previously received treatment but experienced disease progression or inadequate response. Participants will undergo regular hospital or clinic visits for comprehensive monitoring including medical assessments, blood tests, adverse event evaluation, and quality-of-life questionnaires, with the primary objective being to assess changes in disease symptoms and overall treatment efficacy compared to current standard care options.', short_summary = 'This is a 3.5-year clinical trial comparing the investigational drug etentamig to standard available therapies in approximately 380 adult participants with relapsed or refractory multiple myeloma. The study will evaluate whether etentamig monotherapy can improve disease symptoms compared to current standard treatments across approximately 140 global sites.'
WHERE nct_id = 'NCT06158841';


-- ================================================================
-- End of file. Total statements: 2
-- ================================================================
