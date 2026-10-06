-- Letoryn R1: seed initial platform operator (run AFTER EF migrations).
-- Aligned with EF migration InitialPlatformFoundation ("UserIdentities" table).
--
-- Usage:
--   1. Replace ENTRA_OID and EMAIL below.
--   2. Connect to the letoryn database (aspire describe apiservice --format Json →
--      ConnectionStrings__letoryn or LETORYN_URI; not aspire describe postgres alone).
--   3. psql "<letoryn-connection-string>" -f scripts/r1/seed-platform-operator.sql

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
