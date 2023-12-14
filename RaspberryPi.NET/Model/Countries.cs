using System;
using System.Linq;

namespace RaspberryPi
{
    public static class Countries
    {
        /// <summary>
        ///     Obtain ISO3166-1 Country based on its alpha2 code.
        /// </summary>
        public static Country FromAlpha2(string alpha2)
        {
            var country = All.FirstOrDefault(p => p.Alpha2 == alpha2);
            if (country == default)
            {
                throw new ArgumentException($"Alpha2 '{alpha2}' does not map to a country", nameof(alpha2));
            }

            return country;
        }

        /// <summary>
        ///     Obtain ISO3166-1 Country based on its alpha3 code.
        /// </summary>
        public static Country FromAlpha3(string alpha3)
        {
            var country = All.FirstOrDefault(p => p.Alpha3 == alpha3);
            if (country == default)
            {
                throw new ArgumentException($"Alpha3 '{alpha3}' does not map to a country", nameof(alpha3));
            }

            return country;
        }

        public static readonly Country Afghanistan = new Country
        {
            Name = "Afghanistan",
            Alpha2 = "AF",
            Alpha3 = "AFG",
            NumericCode = 4
        };

        public static readonly Country ÅlandIslands = new Country
        {
            Name = "Åland Islands",
            Alpha2 = "AX",
            Alpha3 = "ALA",
            NumericCode = 248
        };

        public static readonly Country Albania = new Country
        {
            Name = "Albania",
            Alpha2 = "AL",
            Alpha3 = "ALB",
            NumericCode = 8
        };

        public static readonly Country Algeria = new Country
        {
            Name = "Algeria",
            Alpha2 = "DZ",
            Alpha3 = "DZA",
            NumericCode = 12
        };

        public static readonly Country Switzerland = new Country
        {
            Name = "Switzerland",
            Alpha2 = "CH",
            Alpha3 = "CHE",
            NumericCode = 756
        };

        public static readonly Country Germany = new Country
        {
            Name = "Germany",
            Alpha2 = "DE",
            Alpha3 = "DEU",
            NumericCode = 276
        };

        public static readonly Country[] All =
        {
            Afghanistan,
            ÅlandIslands,
            Albania,
            Algeria,
            new Country
            {
                Name = "American Samoa",
                Alpha2 = "AS",
                Alpha3 = "ASM",
                NumericCode = 16
            },
            new Country
            {
                Name = "Andorra",
                Alpha2 = "AD",
                Alpha3 = "AND",
                NumericCode = 20
            },
            new Country
            {
                Name = "Angola",
                Alpha2 = "AO",
                Alpha3 = "AGO",
                NumericCode = 24
            },
            new Country
            {
                Name = "Anguilla",
                Alpha2 = "AI",
                Alpha3 = "AIA",
                NumericCode = 660
            },
            new Country
            {
                Name = "Antarctica",
                Alpha2 = "AQ",
                Alpha3 = "ATA",
                NumericCode = 10
            },
            new Country
            {
                Name = "Antigua and Barbuda",
                Alpha2 = "AG",
                Alpha3 = "ATG",
                NumericCode = 28
            },
            new Country
            {
                Name = "Argentina",
                Alpha2 = "AR",
                Alpha3 = "ARG",
                NumericCode = 32
            },
            new Country
            {
                Name = "Armenia",
                Alpha2 = "AM",
                Alpha3 = "ARM",
                NumericCode = 51
            },
            new Country
            {
                Name = "Aruba",
                Alpha2 = "AW",
                Alpha3 = "ABW",
                NumericCode = 533
            },
            new Country
            {
                Name = "Australia",
                Alpha2 = "AU",
                Alpha3 = "AUS",
                NumericCode = 36
            },
            new Country
            {
                Name = "Austria",
                Alpha2 = "AT",
                Alpha3 = "AUT",
                NumericCode = 40
            },
            new Country
            {
                Name = "Azerbaijan",
                Alpha2 = "AZ",
                Alpha3 = "AZE",
                NumericCode = 31
            },
            new Country
            {
                Name = "Bahamas",
                Alpha2 = "BS",
                Alpha3 = "BHS",
                NumericCode = 44
            },
            new Country
            {
                Name = "Bahrain",
                Alpha2 = "BH",
                Alpha3 = "BHR",
                NumericCode = 48
            },
            new Country
            {
                Name = "Bangladesh",
                Alpha2 = "BD",
                Alpha3 = "BGD",
                NumericCode = 50
            },
            new Country
            {
                Name = "Barbados",
                Alpha2 = "BB",
                Alpha3 = "BRB",
                NumericCode = 52
            },
            new Country
            {
                Name = "Belarus",
                Alpha2 = "BY",
                Alpha3 = "BLR",
                NumericCode = 112
            },
            new Country
            {
                Name = "Belgium",
                Alpha2 = "BE",
                Alpha3 = "BEL",
                NumericCode = 56
            },
            new Country
            {
                Name = "Belize",
                Alpha2 = "BZ",
                Alpha3 = "BLZ",
                NumericCode = 84
            },
            new Country
            {
                Name = "Benin",
                Alpha2 = "BJ",
                Alpha3 = "BEN",
                NumericCode = 204
            },
            new Country
            {
                Name = "Bermuda",
                Alpha2 = "BM",
                Alpha3 = "BMU",
                NumericCode = 60
            },
            new Country
            {
                Name = "Bhutan",
                Alpha2 = "BT",
                Alpha3 = "BTN",
                NumericCode = 64
            },
            new Country
            {
                Name = "Bolivia (Plurinational State of)",
                Alpha2 = "BO",
                Alpha3 = "BOL",
                NumericCode = 68
            },
            new Country
            {
                Name = "Bonaire, Sint Eustatius and Saba",
                Alpha2 = "BQ",
                Alpha3 = "BES",
                NumericCode = 535
            },
            new Country
            {
                Name = "Bosnia and Herzegovina",
                Alpha2 = "BA",
                Alpha3 = "BIH",
                NumericCode = 70
            },
            new Country
            {
                Name = "Botswana",
                Alpha2 = "BW",
                Alpha3 = "BWA",
                NumericCode = 72
            },
            new Country
            {
                Name = "Bouvet Island",
                Alpha2 = "BV",
                Alpha3 = "BVT",
                NumericCode = 74
            },
            new Country
            {
                Name = "Brazil",
                Alpha2 = "BR",
                Alpha3 = "BRA",
                NumericCode = 76
            },
            new Country
            {
                Name = "British Indian Ocean Territory",
                Alpha2 = "IO",
                Alpha3 = "IOT",
                NumericCode = 86
            },
            new Country
            {
                Name = "Brunei Darussalam",
                Alpha2 = "BN",
                Alpha3 = "BRN",
                NumericCode = 96
            },
            new Country
            {
                Name = "Bulgaria",
                Alpha2 = "BG",
                Alpha3 = "BGR",
                NumericCode = 100
            },
            new Country
            {
                Name = "Burkina Faso",
                Alpha2 = "BF",
                Alpha3 = "BFA",
                NumericCode = 854
            },
            new Country
            {
                Name = "Burundi",
                Alpha2 = "BI",
                Alpha3 = "BDI",
                NumericCode = 108
            },
            new Country
            {
                Name = "Cabo Verde",
                Alpha2 = "CV",
                Alpha3 = "CPV",
                NumericCode = 132
            },
            new Country
            {
                Name = "Cambodia",
                Alpha2 = "KH",
                Alpha3 = "KHM",
                NumericCode = 116
            },
            new Country
            {
                Name = "Cameroon",
                Alpha2 = "CM",
                Alpha3 = "CMR",
                NumericCode = 120
            },
            new Country
            {
                Name = "Canada",
                Alpha2 = "CA",
                Alpha3 = "CAN",
                NumericCode = 124
            },
            new Country
            {
                Name = "Cayman Islands",
                Alpha2 = "KY",
                Alpha3 = "CYM",
                NumericCode = 136
            },
            new Country
            {
                Name = "Central African Republic",
                Alpha2 = "CF",
                Alpha3 = "CAF",
                NumericCode = 140
            },
            new Country
            {
                Name = "Chad",
                Alpha2 = "TD",
                Alpha3 = "TCD",
                NumericCode = 148
            },
            new Country
            {
                Name = "Chile",
                Alpha2 = "CL",
                Alpha3 = "CHL",
                NumericCode = 152
            },
            new Country
            {
                Name = "China",
                Alpha2 = "CN",
                Alpha3 = "CHN",
                NumericCode = 156
            },
            new Country
            {
                Name = "Christmas Island",
                Alpha2 = "CX",
                Alpha3 = "CXR",
                NumericCode = 162
            },
            new Country
            {
                Name = "Cocos (Keeling) Islands",
                Alpha2 = "CC",
                Alpha3 = "CCK",
                NumericCode = 166
            },
            new Country
            {
                Name = "Colombia",
                Alpha2 = "CO",
                Alpha3 = "COL",
                NumericCode = 170
            },
            new Country
            {
                Name = "Comoros",
                Alpha2 = "KM",
                Alpha3 = "COM",
                NumericCode = 174
            },
            new Country
            {
                Name = "Congo",
                Alpha2 = "CG",
                Alpha3 = "COG",
                NumericCode = 178
            },
            new Country
            {
                Name = "Congo (Democratic Republic of the)",
                Alpha2 = "CD",
                Alpha3 = "COD",
                NumericCode = 180
            },
            new Country
            {
                Name = "Cook Islands",
                Alpha2 = "CK",
                Alpha3 = "COK",
                NumericCode = 184
            },
            new Country
            {
                Name = "Costa Rica",
                Alpha2 = "CR",
                Alpha3 = "CRI",
                NumericCode = 188
            },
            new Country
            {
                Name = "Côte d'Ivoire",
                Alpha2 = "CI",
                Alpha3 = "CIV",
                NumericCode = 384
            },
            new Country
            {
                Name = "Croatia",
                Alpha2 = "HR",
                Alpha3 = "HRV",
                NumericCode = 191
            },
            new Country
            {
                Name = "Cuba",
                Alpha2 = "CU",
                Alpha3 = "CUB",
                NumericCode = 192
            },
            new Country
            {
                Name = "Curaçao",
                Alpha2 = "CW",
                Alpha3 = "CUW",
                NumericCode = 531
            },
            new Country
            {
                Name = "Cyprus",
                Alpha2 = "CY",
                Alpha3 = "CYP",
                NumericCode = 196
            },
            new Country
            {
                Name = "Czech Republic",
                Alpha2 = "CZ",
                Alpha3 = "CZE",
                NumericCode = 203
            },
            new Country
            {
                Name = "Denmark",
                Alpha2 = "DK",
                Alpha3 = "DNK",
                NumericCode = 208
            },
            new Country
            {
                Name = "Djibouti",
                Alpha2 = "DJ",
                Alpha3 = "DJI",
                NumericCode = 262
            },
            new Country
            {
                Name = "Dominica",
                Alpha2 = "DM",
                Alpha3 = "DMA",
                NumericCode = 212
            },
            new Country
            {
                Name = "Dominican Republic",
                Alpha2 = "DO",
                Alpha3 = "DOM",
                NumericCode = 214
            },
            new Country
            {
                Name = "Ecuador",
                Alpha2 = "EC",
                Alpha3 = "ECU",
                NumericCode = 218
            },
            new Country
            {
                Name = "Egypt",
                Alpha2 = "EG",
                Alpha3 = "EGY",
                NumericCode = 818
            },
            new Country
            {
                Name = "El Salvador",
                Alpha2 = "SV",
                Alpha3 = "SLV",
                NumericCode = 222
            },
            new Country
            {
                Name = "Equatorial Guinea",
                Alpha2 = "GQ",
                Alpha3 = "GNQ",
                NumericCode = 226
            },
            new Country
            {
                Name = "Eritrea",
                Alpha2 = "ER",
                Alpha3 = "ERI",
                NumericCode = 232
            },
            new Country
            {
                Name = "Estonia",
                Alpha2 = "EE",
                Alpha3 = "EST",
                NumericCode = 233
            },
            new Country
            {
                Name = "Ethiopia",
                Alpha2 = "ET",
                Alpha3 = "ETH",
                NumericCode = 231
            },
            new Country
            {
                Name = "Falkland Islands (Malvinas)",
                Alpha2 = "FK",
                Alpha3 = "FLK",
                NumericCode = 238
            },
            new Country
            {
                Name = "Faroe Islands",
                Alpha2 = "FO",
                Alpha3 = "FRO",
                NumericCode = 234
            },
            new Country
            {
                Name = "Fiji",
                Alpha2 = "FJ",
                Alpha3 = "FJI",
                NumericCode = 242
            },
            new Country
            {
                Name = "Finland",
                Alpha2 = "FI",
                Alpha3 = "FIN",
                NumericCode = 246
            },
            new Country
            {
                Name = "France",
                Alpha2 = "FR",
                Alpha3 = "FRA",
                NumericCode = 250
            },
            new Country
            {
                Name = "French Guiana",
                Alpha2 = "GF",
                Alpha3 = "GUF",
                NumericCode = 254
            },
            new Country
            {
                Name = "French Polynesia",
                Alpha2 = "PF",
                Alpha3 = "PYF",
                NumericCode = 258
            },
            new Country
            {
                Name = "French Southern Territories",
                Alpha2 = "TF",
                Alpha3 = "ATF",
                NumericCode = 260
            },
            new Country
            {
                Name = "Gabon",
                Alpha2 = "GA",
                Alpha3 = "GAB",
                NumericCode = 266
            },
            new Country
            {
                Name = "Gambia",
                Alpha2 = "GM",
                Alpha3 = "GMB",
                NumericCode = 270
            },
            new Country
            {
                Name = "Georgia",
                Alpha2 = "GE",
                Alpha3 = "GEO",
                NumericCode = 268
            },
            Germany,
            new Country
            {
                Name = "Ghana",
                Alpha2 = "GH",
                Alpha3 = "GHA",
                NumericCode = 288
            },
            new Country
            {
                Name = "Gibraltar",
                Alpha2 = "GI",
                Alpha3 = "GIB",
                NumericCode = 292
            },
            new Country
            {
                Name = "Greece",
                Alpha2 = "GR",
                Alpha3 = "GRC",
                NumericCode = 300
            },
            new Country
            {
                Name = "Greenland",
                Alpha2 = "GL",
                Alpha3 = "GRL",
                NumericCode = 304
            },
            new Country
            {
                Name = "Grenada",
                Alpha2 = "GD",
                Alpha3 = "GRD",
                NumericCode = 308
            },
            new Country
            {
                Name = "Guadeloupe",
                Alpha2 = "GP",
                Alpha3 = "GLP",
                NumericCode = 312
            },
            new Country
            {
                Name = "Guam",
                Alpha2 = "GU",
                Alpha3 = "GUM",
                NumericCode = 316
            },
            new Country
            {
                Name = "Guatemala",
                Alpha2 = "GT",
                Alpha3 = "GTM",
                NumericCode = 320
            },
            new Country
            {
                Name = "Guernsey",
                Alpha2 = "GG",
                Alpha3 = "GGY",
                NumericCode = 831
            },
            new Country
            {
                Name = "Guinea",
                Alpha2 = "GN",
                Alpha3 = "GIN",
                NumericCode = 324
            },
            new Country
            {
                Name = "Guinea-Bissau",
                Alpha2 = "GW",
                Alpha3 = "GNB",
                NumericCode = 624
            },
            new Country
            {
                Name = "Guyana",
                Alpha2 = "GY",
                Alpha3 = "GUY",
                NumericCode = 328
            },
            new Country
            {
                Name = "Haiti",
                Alpha2 = "HT",
                Alpha3 = "HTI",
                NumericCode = 332
            },
            new Country
            {
                Name = "Heard Island and McDonald Islands",
                Alpha2 = "HM",
                Alpha3 = "HMD",
                NumericCode = 334
            },
            new Country
            {
                Name = "Holy See",
                Alpha2 = "VA",
                Alpha3 = "VAT",
                NumericCode = 336
            },
            new Country
            {
                Name = "Honduras",
                Alpha2 = "HN",
                Alpha3 = "HND",
                NumericCode = 340
            },
            new Country
            {
                Name = "Hong Kong",
                Alpha2 = "HK",
                Alpha3 = "HKG",
                NumericCode = 344
            },
            new Country
            {
                Name = "Hungary",
                Alpha2 = "HU",
                Alpha3 = "HUN",
                NumericCode = 348
            },
            new Country
            {
                Name = "Iceland",
                Alpha2 = "IS",
                Alpha3 = "ISL",
                NumericCode = 352
            },
            new Country
            {
                Name = "India",
                Alpha2 = "IN",
                Alpha3 = "IND",
                NumericCode = 356
            },
            new Country
            {
                Name = "Indonesia",
                Alpha2 = "ID",
                Alpha3 = "IDN",
                NumericCode = 360
            },
            new Country
            {
                Name = "Iran (Islamic Republic of)",
                Alpha2 = "IR",
                Alpha3 = "IRN",
                NumericCode = 364
            },
            new Country
            {
                Name = "Iraq",
                Alpha2 = "IQ",
                Alpha3 = "IRQ",
                NumericCode = 368
            },
            new Country
            {
                Name = "Ireland",
                Alpha2 = "IE",
                Alpha3 = "IRL",
                NumericCode = 372
            },
            new Country
            {
                Name = "Isle of Man",
                Alpha2 = "IM",
                Alpha3 = "IMN",
                NumericCode = 833
            },
            new Country
            {
                Name = "Israel",
                Alpha2 = "IL",
                Alpha3 = "ISR",
                NumericCode = 376
            },
            new Country
            {
                Name = "Italy",
                Alpha2 = "IT",
                Alpha3 = "ITA",
                NumericCode = 380
            },
            new Country
            {
                Name = "Jamaica",
                Alpha2 = "JM",
                Alpha3 = "JAM",
                NumericCode = 388
            },
            new Country
            {
                Name = "Japan",
                Alpha2 = "JP",
                Alpha3 = "JPN",
                NumericCode = 392
            },
            new Country
            {
                Name = "Jersey",
                Alpha2 = "JE",
                Alpha3 = "JEY",
                NumericCode = 832
            },
            new Country
            {
                Name = "Jordan",
                Alpha2 = "JO",
                Alpha3 = "JOR",
                NumericCode = 400
            },
            new Country
            {
                Name = "Kazakhstan",
                Alpha2 = "KZ",
                Alpha3 = "KAZ",
                NumericCode = 398
            },
            new Country
            {
                Name = "Kenya",
                Alpha2 = "KE",
                Alpha3 = "KEN",
                NumericCode = 404
            },
            new Country
            {
                Name = "Kiribati",
                Alpha2 = "KI",
                Alpha3 = "KIR",
                NumericCode = 296
            },
            new Country
            {
                Name = "Korea (Democratic People's Republic of)",
                Alpha2 = "KP",
                Alpha3 = "PRK",
                NumericCode = 408
            },
            new Country
            {
                Name = "Korea (Republic of)",
                Alpha2 = "KR",
                Alpha3 = "KOR",
                NumericCode = 410
            },
            new Country
            {
                Name = "Kuwait",
                Alpha2 = "KW",
                Alpha3 = "KWT",
                NumericCode = 414
            },
            new Country
            {
                Name = "Kyrgyzstan",
                Alpha2 = "KG",
                Alpha3 = "KGZ",
                NumericCode = 417
            },
            new Country
            {
                Name = "Lao People's Democratic Republic",
                Alpha2 = "LA",
                Alpha3 = "LAO",
                NumericCode = 418
            },
            new Country
            {
                Name = "Latvia",
                Alpha2 = "LV",
                Alpha3 = "LVA",
                NumericCode = 428
            },
            new Country
            {
                Name = "Lebanon",
                Alpha2 = "LB",
                Alpha3 = "LBN",
                NumericCode = 422
            },
            new Country
            {
                Name = "Lesotho",
                Alpha2 = "LS",
                Alpha3 = "LSO",
                NumericCode = 426
            },
            new Country
            {
                Name = "Liberia",
                Alpha2 = "LR",
                Alpha3 = "LBR",
                NumericCode = 430
            },
            new Country
            {
                Name = "Libya",
                Alpha2 = "LY",
                Alpha3 = "LBY",
                NumericCode = 434
            },
            new Country
            {
                Name = "Liechtenstein",
                Alpha2 = "LI",
                Alpha3 = "LIE",
                NumericCode = 438
            },
            new Country
            {
                Name = "Lithuania",
                Alpha2 = "LT",
                Alpha3 = "LTU",
                NumericCode = 440
            },
            new Country
            {
                Name = "Luxembourg",
                Alpha2 = "LU",
                Alpha3 = "LUX",
                NumericCode = 442
            },
            new Country
            {
                Name = "Macao",
                Alpha2 = "MO",
                Alpha3 = "MAC",
                NumericCode = 446
            },
            new Country
            {
                Name = "Macedonia (the former Yugoslav Republic of)",
                Alpha2 = "MK",
                Alpha3 = "MKD",
                NumericCode = 807
            },
            new Country
            {
                Name = "Madagascar",
                Alpha2 = "MG",
                Alpha3 = "MDG",
                NumericCode = 450
            },
            new Country
            {
                Name = "Malawi",
                Alpha2 = "MW",
                Alpha3 = "MWI",
                NumericCode = 454
            },
            new Country
            {
                Name = "Malaysia",
                Alpha2 = "MY",
                Alpha3 = "MYS",
                NumericCode = 458
            },
            new Country
            {
                Name = "Maldives",
                Alpha2 = "MV",
                Alpha3 = "MDV",
                NumericCode = 462
            },
            new Country
            {
                Name = "Mali",
                Alpha2 = "ML",
                Alpha3 = "MLI",
                NumericCode = 466
            },
            new Country
            {
                Name = "Malta",
                Alpha2 = "MT",
                Alpha3 = "MLT",
                NumericCode = 470
            },
            new Country
            {
                Name = "Marshall Islands",
                Alpha2 = "MH",
                Alpha3 = "MHL",
                NumericCode = 584
            },
            new Country
            {
                Name = "Martinique",
                Alpha2 = "MQ",
                Alpha3 = "MTQ",
                NumericCode = 474
            },
            new Country
            {
                Name = "Mauritania",
                Alpha2 = "MR",
                Alpha3 = "MRT",
                NumericCode = 478
            },
            new Country
            {
                Name = "Mauritius",
                Alpha2 = "MU",
                Alpha3 = "MUS",
                NumericCode = 480
            },
            new Country
            {
                Name = "Mayotte",
                Alpha2 = "YT",
                Alpha3 = "MYT",
                NumericCode = 175
            },
            new Country
            {
                Name = "Mexico",
                Alpha2 = "MX",
                Alpha3 = "MEX",
                NumericCode = 484
            },
            new Country
            {
                Name = "Micronesia (Federated States of)",
                Alpha2 = "FM",
                Alpha3 = "FSM",
                NumericCode = 583
            },
            new Country
            {
                Name = "Moldova (Republic of)",
                Alpha2 = "MD",
                Alpha3 = "MDA",
                NumericCode = 498
            },
            new Country
            {
                Name = "Monaco",
                Alpha2 = "MC",
                Alpha3 = "MCO",
                NumericCode = 492
            },
            new Country
            {
                Name = "Mongolia",
                Alpha2 = "MN",
                Alpha3 = "MNG",
                NumericCode = 496
            },
            new Country
            {
                Name = "Montenegro",
                Alpha2 = "ME",
                Alpha3 = "MNE",
                NumericCode = 499
            },
            new Country
            {
                Name = "Montserrat",
                Alpha2 = "MS",
                Alpha3 = "MSR",
                NumericCode = 500
            },
            new Country
            {
                Name = "Morocco",
                Alpha2 = "MA",
                Alpha3 = "MAR",
                NumericCode = 504
            },
            new Country
            {
                Name = "Mozambique",
                Alpha2 = "MZ",
                Alpha3 = "MOZ",
                NumericCode = 508
            },
            new Country
            {
                Name = "Myanmar",
                Alpha2 = "MM",
                Alpha3 = "MMR",
                NumericCode = 104
            },
            new Country
            {
                Name = "Namibia",
                Alpha2 = "NA",
                Alpha3 = "NAM",
                NumericCode = 516
            },
            new Country
            {
                Name = "Nauru",
                Alpha2 = "NR",
                Alpha3 = "NRU",
                NumericCode = 520
            },
            new Country
            {
                Name = "Nepal",
                Alpha2 = "NP",
                Alpha3 = "NPL",
                NumericCode = 524
            },
            new Country
            {
                Name = "Netherlands",
                Alpha2 = "NL",
                Alpha3 = "NLD",
                NumericCode = 528
            },
            new Country
            {
                Name = "New Caledonia",
                Alpha2 = "NC",
                Alpha3 = "NCL",
                NumericCode = 540
            },
            new Country
            {
                Name = "New Zealand",
                Alpha2 = "NZ",
                Alpha3 = "NZL",
                NumericCode = 554
            },
            new Country
            {
                Name = "Nicaragua",
                Alpha2 = "NI",
                Alpha3 = "NIC",
                NumericCode = 558
            },
            new Country
            {
                Name = "Niger",
                Alpha2 = "NE",
                Alpha3 = "NER",
                NumericCode = 562
            },
            new Country
            {
                Name = "Nigeria",
                Alpha2 = "NG",
                Alpha3 = "NGA",
                NumericCode = 566
            },
            new Country
            {
                Name = "Niue",
                Alpha2 = "NU",
                Alpha3 = "NIU",
                NumericCode = 570
            },
            new Country
            {
                Name = "Norfolk Island",
                Alpha2 = "NF",
                Alpha3 = "NFK",
                NumericCode = 574
            },
            new Country
            {
                Name = "Northern Mariana Islands",
                Alpha2 = "MP",
                Alpha3 = "MNP",
                NumericCode = 580
            },
            new Country
            {
                Name = "Norway",
                Alpha2 = "NO",
                Alpha3 = "NOR",
                NumericCode = 578
            },
            new Country
            {
                Name = "Oman",
                Alpha2 = "OM",
                Alpha3 = "OMN",
                NumericCode = 512
            },
            new Country
            {
                Name = "Pakistan",
                Alpha2 = "PK",
                Alpha3 = "PAK",
                NumericCode = 586
            },
            new Country
            {
                Name = "Palau",
                Alpha2 = "PW",
                Alpha3 = "PLW",
                NumericCode = 585
            },
            new Country
            {
                Name = "Palestine, State of",
                Alpha2 = "PS",
                Alpha3 = "PSE",
                NumericCode = 275
            },
            new Country
            {
                Name = "Panama",
                Alpha2 = "PA",
                Alpha3 = "PAN",
                NumericCode = 591
            },
            new Country
            {
                Name = "Papua New Guinea",
                Alpha2 = "PG",
                Alpha3 = "PNG",
                NumericCode = 598
            },
            new Country
            {
                Name = "Paraguay",
                Alpha2 = "PY",
                Alpha3 = "PRY",
                NumericCode = 600
            },
            new Country
            {
                Name = "Peru",
                Alpha2 = "PE",
                Alpha3 = "PER",
                NumericCode = 604
            },
            new Country
            {
                Name = "Philippines",
                Alpha2 = "PH",
                Alpha3 = "PHL",
                NumericCode = 608
            },
            new Country
            {
                Name = "Pitcairn",
                Alpha2 = "PN",
                Alpha3 = "PCN",
                NumericCode = 612
            },
            new Country
            {
                Name = "Poland",
                Alpha2 = "PL",
                Alpha3 = "POL",
                NumericCode = 616
            },
            new Country
            {
                Name = "Portugal",
                Alpha2 = "PT",
                Alpha3 = "PRT",
                NumericCode = 620
            },
            new Country
            {
                Name = "Puerto Rico",
                Alpha2 = "PR",
                Alpha3 = "PRI",
                NumericCode = 630
            },
            new Country
            {
                Name = "Qatar",
                Alpha2 = "QA",
                Alpha3 = "QAT",
                NumericCode = 634
            },
            new Country
            {
                Name = "Réunion",
                Alpha2 = "RE",
                Alpha3 = "REU",
                NumericCode = 638
            },
            new Country
            {
                Name = "Romania",
                Alpha2 = "RO",
                Alpha3 = "ROU",
                NumericCode = 642
            },
            new Country
            {
                Name = "Russian Federation",
                Alpha2 = "RU",
                Alpha3 = "RUS",
                NumericCode = 643
            },
            new Country
            {
                Name = "Rwanda",
                Alpha2 = "RW",
                Alpha3 = "RWA",
                NumericCode = 646
            },
            new Country
            {
                Name = "Saint Barthélemy",
                Alpha2 = "BL",
                Alpha3 = "BLM",
                NumericCode = 652
            },
            new Country
            {
                Name = "Saint Helena, Ascension and Tristan da Cunha",
                Alpha2 = "SH",
                Alpha3 = "SHN",
                NumericCode = 654
            },
            new Country
            {
                Name = "Saint Kitts and Nevis",
                Alpha2 = "KN",
                Alpha3 = "KNA",
                NumericCode = 659
            },
            new Country
            {
                Name = "Saint Lucia",
                Alpha2 = "LC",
                Alpha3 = "LCA",
                NumericCode = 662
            },
            new Country
            {
                Name = "Saint Martin (French part)",
                Alpha2 = "MF",
                Alpha3 = "MAF",
                NumericCode = 663
            },
            new Country
            {
                Name = "Saint Pierre and Miquelon",
                Alpha2 = "PM",
                Alpha3 = "SPM",
                NumericCode = 666
            },
            new Country
            {
                Name = "Saint Vincent and the Grenadines",
                Alpha2 = "VC",
                Alpha3 = "VCT",
                NumericCode = 670
            },
            new Country
            {
                Name = "Samoa",
                Alpha2 = "WS",
                Alpha3 = "WSM",
                NumericCode = 882
            },
            new Country
            {
                Name = "San Marino",
                Alpha2 = "SM",
                Alpha3 = "SMR",
                NumericCode = 674
            },
            new Country
            {
                Name = "Sao Tome and Principe",
                Alpha2 = "ST",
                Alpha3 = "STP",
                NumericCode = 678
            },
            new Country
            {
                Name = "Saudi Arabia",
                Alpha2 = "SA",
                Alpha3 = "SAU",
                NumericCode = 682
            },
            new Country
            {
                Name = "Senegal",
                Alpha2 = "SN",
                Alpha3 = "SEN",
                NumericCode = 686
            },
            new Country
            {
                Name = "Serbia",
                Alpha2 = "RS",
                Alpha3 = "SRB",
                NumericCode = 688
            },
            new Country
            {
                Name = "Seychelles",
                Alpha2 = "SC",
                Alpha3 = "SYC",
                NumericCode = 690
            },
            new Country
            {
                Name = "Sierra Leone",
                Alpha2 = "SL",
                Alpha3 = "SLE",
                NumericCode = 694
            },
            new Country
            {
                Name = "Singapore",
                Alpha2 = "SG",
                Alpha3 = "SGP",
                NumericCode = 702
            },
            new Country
            {
                Name = "Sint Maarten (Dutch part)",
                Alpha2 = "SX",
                Alpha3 = "SXM",
                NumericCode = 534
            },
            new Country
            {
                Name = "Slovakia",
                Alpha2 = "SK",
                Alpha3 = "SVK",
                NumericCode = 703
            },
            new Country
            {
                Name = "Slovenia",
                Alpha2 = "SI",
                Alpha3 = "SVN",
                NumericCode = 705
            },
            new Country
            {
                Name = "Solomon Islands",
                Alpha2 = "SB",
                Alpha3 = "SLB",
                NumericCode = 90
            },
            new Country
            {
                Name = "Somalia",
                Alpha2 = "SO",
                Alpha3 = "SOM",
                NumericCode = 706
            },
            new Country
            {
                Name = "South Africa",
                Alpha2 = "ZA",
                Alpha3 = "ZAF",
                NumericCode = 710
            },
            new Country
            {
                Name = "South Georgia and the South Sandwich Islands",
                Alpha2 = "GS",
                Alpha3 = "SGS",
                NumericCode = 239
            },
            new Country
            {
                Name = "South Sudan",
                Alpha2 = "SS",
                Alpha3 = "SSD",
                NumericCode = 728
            },
            new Country
            {
                Name = "Spain",
                Alpha2 = "ES",
                Alpha3 = "ESP",
                NumericCode = 724
            },
            new Country
            {
                Name = "Sri Lanka",
                Alpha2 = "LK",
                Alpha3 = "LKA",
                NumericCode = 144
            },
            new Country
            {
                Name = "Sudan",
                Alpha2 = "SD",
                Alpha3 = "SDN",
                NumericCode = 729
            },
            new Country
            {
                Name = "Suriname",
                Alpha2 = "SR",
                Alpha3 = "SUR",
                NumericCode = 740
            },
            new Country
            {
                Name = "Svalbard and Jan Mayen",
                Alpha2 = "SJ",
                Alpha3 = "SJM",
                NumericCode = 744
            },
            new Country
            {
                Name = "Swaziland",
                Alpha2 = "SZ",
                Alpha3 = "SWZ",
                NumericCode = 748
            },
            new Country
            {
                Name = "Sweden",
                Alpha2 = "SE",
                Alpha3 = "SWE",
                NumericCode = 752
            },
            Switzerland,
            new Country
            {
                Name = "Syrian Arab Republic",
                Alpha2 = "SY",
                Alpha3 = "SYR",
                NumericCode = 760
            },
            new Country
            {
                Name = "Taiwan, Province of China[a]",
                Alpha2 = "TW",
                Alpha3 = "TWN",
                NumericCode = 158
            },
            new Country
            {
                Name = "Tajikistan",
                Alpha2 = "TJ",
                Alpha3 = "TJK",
                NumericCode = 762
            },
            new Country
            {
                Name = "Tanzania, United Republic of",
                Alpha2 = "TZ",
                Alpha3 = "TZA",
                NumericCode = 834
            },
            new Country
            {
                Name = "Thailand",
                Alpha2 = "TH",
                Alpha3 = "THA",
                NumericCode = 764
            },
            new Country
            {
                Name = "Timor-Leste",
                Alpha2 = "TL",
                Alpha3 = "TLS",
                NumericCode = 626
            },
            new Country
            {
                Name = "Togo",
                Alpha2 = "TG",
                Alpha3 = "TGO",
                NumericCode = 768
            },
            new Country
            {
                Name = "Tokelau",
                Alpha2 = "TK",
                Alpha3 = "TKL",
                NumericCode = 772
            },
            new Country
            {
                Name = "Tonga",
                Alpha2 = "TO",
                Alpha3 = "TON",
                NumericCode = 776
            },
            new Country
            {
                Name = "Trinidad and Tobago",
                Alpha2 = "TT",
                Alpha3 = "TTO",
                NumericCode = 780
            },
            new Country
            {
                Name = "Tunisia",
                Alpha2 = "TN",
                Alpha3 = "TUN",
                NumericCode = 788
            },
            new Country
            {
                Name = "Turkey",
                Alpha2 = "TR",
                Alpha3 = "TUR",
                NumericCode = 792
            },
            new Country
            {
                Name = "Turkmenistan",
                Alpha2 = "TM",
                Alpha3 = "TKM",
                NumericCode = 795
            },
            new Country
            {
                Name = "Turks and Caicos Islands",
                Alpha2 = "TC",
                Alpha3 = "TCA",
                NumericCode = 796
            },
            new Country
            {
                Name = "Tuvalu",
                Alpha2 = "TV",
                Alpha3 = "TUV",
                NumericCode = 798
            },
            new Country
            {
                Name = "Uganda",
                Alpha2 = "UG",
                Alpha3 = "UGA",
                NumericCode = 800
            },
            new Country
            {
                Name = "Ukraine",
                Alpha2 = "UA",
                Alpha3 = "UKR",
                NumericCode = 804
            },
            new Country
            {
                Name = "United Arab Emirates",
                Alpha2 = "AE",
                Alpha3 = "ARE",
                NumericCode = 784
            },
            new Country
            {
                Name = "United Kingdom of Great Britain and Northern Ireland",
                Alpha2 = "GB",
                Alpha3 = "GBR",
                NumericCode = 826
            },
            new Country
            {
                Name = "United States of America",
                Alpha2 = "US",
                Alpha3 = "USA",
                NumericCode = 840
            },
            new Country
            {
                Name = "United States Minor Outlying Islands",
                Alpha2 = "UM",
                Alpha3 = "UMI",
                NumericCode = 581
            },
            new Country
            {
                Name = "Uruguay",
                Alpha2 = "UY",
                Alpha3 = "URY",
                NumericCode = 858
            },
            new Country
            {
                Name = "Uzbekistan",
                Alpha2 = "UZ",
                Alpha3 = "UZB",
                NumericCode = 860
            },
            new Country
            {
                Name = "Vanuatu",
                Alpha2 = "VU",
                Alpha3 = "VUT",
                NumericCode = 548
            },
            new Country
            {
                Name = "Venezuela (Bolivarian Republic of)",
                Alpha2 = "VE",
                Alpha3 = "VEN",
                NumericCode = 862
            },
            new Country
            {
                Name = "Viet Nam",
                Alpha2 = "VN",
                Alpha3 = "VNM",
                NumericCode = 704
            },
            new Country
            {
                Name = "Virgin Islands (British)",
                Alpha2 = "VG",
                Alpha3 = "VGB",
                NumericCode = 92
            },
            new Country
            {
                Name = "Virgin Islands (U.S.)",
                Alpha2 = "VI",
                Alpha3 = "VIR",
                NumericCode = 850
            },
            new Country
            {
                Name = "Wallis and Futuna",
                Alpha2 = "WF",
                Alpha3 = "WLF",
                NumericCode = 876
            },
            new Country
            {
                Name = "Western Sahara",
                Alpha2 = "EH",
                Alpha3 = "ESH",
                NumericCode = 732
            },
            new Country
            {
                Name = "Yemen",
                Alpha2 = "YE",
                Alpha3 = "YEM",
                NumericCode = 887
            },
            new Country
            {
                Name = "Zambia",
                Alpha2 = "ZM",
                Alpha3 = "ZMB",
                NumericCode = 894
            },
            new Country
            {
                Name = "Zimbabwe",
                Alpha2 = "ZW",
                Alpha3 = "ZWE",
                NumericCode = 716
            }
        };
    }
}
