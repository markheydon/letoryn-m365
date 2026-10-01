-- Tenancy Hub R1: seed initial platform operator (run AFTER EF migrations).
-- Table/column names are placeholders until T102 aligns with the first migration.
--
-- Usage:
--   1. Replace ENTRA_OID and EMAIL below.
--   2. Connect to Aspire Postgres (`aspire describe postgres`).
--   3. psql "<connection-string>" -f scripts/r1/seed-platform-operator.sql

DO $$
DECLARE
    entra_oid text := 'REPLACE_WITH_ENTRA_OID';
    email text := lower(trim('REPLACE_WITH_EMAIL'));
BEGIN
    INSERT INTO "UserIdentities" (
        "Id",
        "EntraObjectId",
        "Email",
        "IsPlatformOperator",
        "LastUsedAgencyId",
        "CreatedAt"
    )
    VALUES (
        gen_random_uuid(),
        entra_oid,
        email,
        TRUE,
        NULL,
        (NOW() AT TIME ZONE 'UTC')
    )
    ON CONFLICT ("EntraObjectId") DO UPDATE
    SET
        "IsPlatformOperator" = TRUE,
        "Email" = EXCLUDED."Email";
END $$;
