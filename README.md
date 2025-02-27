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