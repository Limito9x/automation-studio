using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Pipeline.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigratePinEnumsToStringAndCleanVariable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    nd RECORD;
                    p RECORD;
                    item jsonb;
                    new_inputs jsonb;
                    new_outputs jsonb;
                    new_vars jsonb;
                    prim_map jsonb := '{"0": "String", "1": "Number", "2": "Boolean", "3": "Path", "4": "EntityRef", "5": "Asset", "6": "EntityRef"}';
                    card_map jsonb := '{"0": "Single", "1": "Array", "2": "Map"}';
                    kind_map jsonb := '{"0": "Data", "1": "Exec"}';
                    pt text;
                    card text;
                    kd text;
                BEGIN
                    -- 1. Update NodeDefinitions
                    FOR nd IN SELECT "Id", "Inputs", "Outputs" FROM pipeline."NodeDefinitions" LOOP
                        new_inputs := '[]'::jsonb;
                        IF nd."Inputs" IS NOT NULL AND jsonb_typeof(nd."Inputs") = 'array' THEN
                            FOR item IN SELECT * FROM jsonb_array_elements(nd."Inputs") LOOP
                                pt := item->>'PrimitiveType';
                                IF prim_map ? pt THEN
                                    item := jsonb_set(item, '{PrimitiveType}', prim_map->pt);
                                    IF pt = '6' THEN
                                        item := jsonb_set(item, '{EntityTarget}', '"variable"'::jsonb);
                                    END IF;
                                END IF;
                                card := item->>'Cardinality';
                                IF card_map ? card THEN
                                    item := jsonb_set(item, '{Cardinality}', card_map->card);
                                END IF;
                                kd := item->>'Kind';
                                IF kind_map ? kd THEN
                                    item := jsonb_set(item, '{Kind}', kind_map->kd);
                                END IF;
                                new_inputs := new_inputs || jsonb_build_array(item);
                            END LOOP;
                        ELSE
                            new_inputs := nd."Inputs";
                        END IF;

                        new_outputs := '[]'::jsonb;
                        IF nd."Outputs" IS NOT NULL AND jsonb_typeof(nd."Outputs") = 'array' THEN
                            FOR item IN SELECT * FROM jsonb_array_elements(nd."Outputs") LOOP
                                pt := item->>'PrimitiveType';
                                IF prim_map ? pt THEN
                                    item := jsonb_set(item, '{PrimitiveType}', prim_map->pt);
                                    IF pt = '6' THEN
                                        item := jsonb_set(item, '{EntityTarget}', '"variable"'::jsonb);
                                    END IF;
                                END IF;
                                card := item->>'Cardinality';
                                IF card_map ? card THEN
                                    item := jsonb_set(item, '{Cardinality}', card_map->card);
                                END IF;
                                kd := item->>'Kind';
                                IF kind_map ? kd THEN
                                    item := jsonb_set(item, '{Kind}', kind_map->kd);
                                END IF;
                                new_outputs := new_outputs || jsonb_build_array(item);
                            END LOOP;
                        ELSE
                            new_outputs := nd."Outputs";
                        END IF;

                        UPDATE pipeline."NodeDefinitions"
                        SET "Inputs" = new_inputs, "Outputs" = new_outputs
                        WHERE "Id" = nd."Id";
                    END LOOP;

                    -- 2. Update Pipelines Variables
                    FOR p IN SELECT "Id", "Variables" FROM pipeline."Pipelines" WHERE "Variables" IS NOT NULL LOOP
                        new_vars := '[]'::jsonb;
                        IF jsonb_typeof(p."Variables") = 'array' THEN
                            FOR item IN SELECT * FROM jsonb_array_elements(p."Variables") LOOP
                                pt := item->>'Type';
                                IF prim_map ? pt THEN
                                    IF pt = '6' THEN
                                        item := jsonb_set(item, '{Type}', '"String"'::jsonb);
                                    ELSE
                                        item := jsonb_set(item, '{Type}', prim_map->pt);
                                    END IF;
                                END IF;
                                card := item->>'Cardinality';
                                IF card_map ? card THEN
                                    item := jsonb_set(item, '{Cardinality}', card_map->card);
                                END IF;
                                new_vars := new_vars || jsonb_build_array(item);
                            END LOOP;
                            UPDATE pipeline."Pipelines"
                            SET "Variables" = new_vars
                            WHERE "Id" = p."Id";
                        END IF;
                    END LOOP;
                END $$;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
