-- O2Ban v0.4 read-only database verification (MariaDB)
SELECT `Group`, COUNT(*) AS option_count
FROM `LookupOptions`
WHERE `IsActive` = 1
GROUP BY `Group`
ORDER BY `Group`;

SELECT COUNT(*) AS users FROM `Users`;
SELECT COUNT(*) AS target_companies FROM `Companies`;
SELECT COUNT(*) AS suppliers FROM `Suppliers`;
SELECT COUNT(*) AS catalog_products FROM `Products`;
SELECT COUNT(*) AS purchase_requirements FROM `Requirements`;
SELECT COUNT(*) AS rfqs FROM `Rfqs`;
SELECT COUNT(*) AS offers FROM `Offers`;
SELECT COUNT(*) AS evidence_records FROM `Evidence`;

-- Products with values not present in reference data (expected: 0)
SELECT p.`Id`, p.`Code`, p.`Category`, p.`Unit`
FROM `Products` p
LEFT JOIN `LookupOptions` c ON c.`Group`='product-category' AND c.`Value`=p.`Category` AND c.`IsActive`=1
LEFT JOIN `LookupOptions` u ON u.`Group`='unit' AND u.`Value`=p.`Unit` AND u.`IsActive`=1
WHERE c.`Id` IS NULL OR u.`Id` IS NULL;

-- Requirements with invalid category, unit or priority (expected: 0)
SELECT r.`Id`, r.`Title`, r.`Category`, r.`Unit`, r.`Priority`
FROM `Requirements` r
LEFT JOIN `LookupOptions` c ON c.`Group`='product-category' AND c.`Value`=r.`Category` AND c.`IsActive`=1
LEFT JOIN `LookupOptions` u ON u.`Group`='unit' AND u.`Value`=r.`Unit` AND u.`IsActive`=1
LEFT JOIN `LookupOptions` p ON p.`Group`='priority' AND p.`Value`=r.`Priority` AND p.`IsActive`=1
WHERE c.`Id` IS NULL OR u.`Id` IS NULL OR p.`Id` IS NULL;
