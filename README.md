# inventorifo
Init Project 26 February 2025


27 February 2025
CREATE TABLE "public"."product_group" ( 
  "id" SERIAL,
  "name" VARCHAR(250) NULL,
  CONSTRAINT "PK_product_group" PRIMARY KEY ("id")
);

CREATE TABLE "public"."unit" ( 
  "id" SERIAL,
  "name" VARCHAR(250) NULL,
  CONSTRAINT "PK_unit" PRIMARY KEY ("id")
);

ALTER TABLE "public"."stock" DROP COLUMN "unit";
ALTER TABLE "public"."stock" ADD  "unit" INT NULL;

1 Maret 2025
ALTER TABLE "public"."stock" ADD  "price_id" BIGSERIAL ;
ALTER TABLE "public"."price" DROP COLUMN "stock_id";
ALTER TABLE "public"."personal" ADD  "national_id_number" VARCHAR(250) NULL;
ALTER TABLE "public"."personal" ADD  "tax_id_number" VARCHAR(250) NULL;
ALTER TABLE "public"."personal" ADD  "health_insurance_id_number" VARCHAR(250) NULL;
ALTER TABLE "public"."personal" RENAME TO "person";
