using api_dotnet.Domain;
using api_dotnet.Data;
using Microsoft.EntityFrameworkCore;

namespace api_dotnet.Data;

public static class Seed
{
    public static async Task EnsureBigSeedAsync(CareflowDb db, int years = 5)
    {
        if (await db.LabResults.AnyAsync()) return;

        db.ChangeTracker.AutoDetectChangesEnabled = false;

        var rand = new Random(42);
        var owners = new List<Owner>(20000);
        var pets   = new List<Pet>(24000);

        // 1) Owners
        for (int i = 0; i < 20000; i++)
        {
            owners.Add(new Owner {
                Name = $"Owner {i:D5}", Phone = $"555-{1000+i:D4}",
                OwnerAddress = new Owner.Address { Street = $"{i} Main St" }
            });
        }
        await BulkAddAsync(db, owners, 2000);

        // 2) Pets
        int petIdSeq = 0;
        foreach (var o in owners)
        {
            int count = 1 + (rand.NextDouble() < 0.2 ? 1 : 0); // 20%는 2마리
            for (int j = 0; j < count; j++)
            {
                pets.Add(new Pet {
                    OwnerId = o.Id,
                    Name = $"Pet {petIdSeq++}",
                    Species = rand.NextDouble() < 0.7 ? "Canine" : "Feline",
                    Breed = null,
                    BirthDate = DateTime.UtcNow.AddDays(-rand.Next(200, 4000))
                });
            }
        }
        await BulkAddAsync(db, pets, 2000);

        // 3) ClinicalNotes (간단 샘플)
        var notesBatch = new List<ClinicalNote>(5000);
        foreach (var p in pets)
        {
            int n = 15 + rand.Next(0, 20); // 평균 25
            for (int k = 0; k < n; k++)
            {
                notesBatch.Add(new ClinicalNote {
                    PetId = p.Id,
                    RecordedAt = DateTimeOffset.UtcNow.AddDays(-rand.Next(0, 365 * years)),
                    Author = "Dr. Smith",
                    Content = "Routine check-up."
                });
                if (notesBatch.Count >= 5000)
                {
                    await BulkAddAsync(db, notesBatch, 5000);
                    notesBatch.Clear();
                }
            }
        }
        if (notesBatch.Count > 0) await BulkAddAsync(db, notesBatch, 5000);

        // 4) LabReports + LabResults
        var reportBatch = new List<LabReport>(2000);
        var resultBatch = new List<LabResult>(40000);

        LabTestType[] types = { LabTestType.CBC, LabTestType.Chemistry, LabTestType.Urinalysis, LabTestType.Thyroid };
        foreach (var p in pets)
        {
            int rcount = 5 + rand.Next(0, 8); // 평균 ~8.3
            for (int r = 0; r < rcount; r++)
            {
                var rpt = new LabReport {
                    PetId = p.Id,
                    TestType = types[rand.Next(types.Length)],
                    CollectedAt = DateTimeOffset.UtcNow.AddDays(-rand.Next(0, 365 * years)),
                    ReportedAt  = DateTimeOffset.UtcNow.AddDays(-rand.Next(0, 365 * years)),
                    LabName = "VetLab", Status = "Final"
                };
                reportBatch.Add(rpt);

                // 항목 수: CBC~20, Chem~18, UA~12, Thyroid~3 (대충 분포)
                int itemCount = rpt.TestType switch {
                    LabTestType.CBC => 18 + rand.Next(0, 6),
                    LabTestType.Chemistry => 16 + rand.Next(0, 6),
                    LabTestType.Urinalysis => 10 + rand.Next(0, 5),
                    LabTestType.Thyroid => 2 + rand.Next(0, 3),
                    _ => 12
                };
                for (int it = 0; it < itemCount; it++)
                {
                    string analyteCode, analyteName, units;
                    decimal valueNumeric, refLow, refHigh;
                    string? flag = null;

                    // 검사 타입별 실제 검사 항목 생성
                    switch (rpt.TestType)
                    {
                        case LabTestType.CBC:
                            (analyteCode, analyteName, units, valueNumeric, refLow, refHigh) = GetCBCAnalyte(it, rand);
                            break;
                        case LabTestType.Chemistry:
                            (analyteCode, analyteName, units, valueNumeric, refLow, refHigh) = GetChemistryAnalyte(it, rand);
                            break;
                        case LabTestType.Urinalysis:
                            (analyteCode, analyteName, units, valueNumeric, refLow, refHigh) = GetUrinalysisAnalyte(it, rand);
                            break;
                        case LabTestType.Thyroid:
                            (analyteCode, analyteName, units, valueNumeric, refLow, refHigh) = GetThyroidAnalyte(it, rand);
                            break;
                        default:
                            analyteCode = $"A{it:00}";
                            analyteName = $"Analyte-{it:00}";
                            units = "U/L";
                            valueNumeric = Math.Round((decimal)(rand.NextDouble() * 100), 2);
                            refLow = 10; refHigh = 90;
                            break;
                    }

                    // 정상범위 밖이면 플래그 설정
                    if (valueNumeric < refLow) flag = "L";
                    else if (valueNumeric > refHigh) flag = "H";

                    resultBatch.Add(new LabResult {
                        Report = rpt,
                        AnalyteCode = analyteCode,
                        AnalyteName = analyteName,
                        ValueNumeric = valueNumeric,
                        Units = units,
                        RefLow = refLow,
                        RefHigh = refHigh,
                        Flag = flag
                    });
                }

                if (reportBatch.Count >= 2000)
                {
                    await BulkAddAsync(db, reportBatch, 2000);
                    await BulkAddAsync(db, resultBatch, 20000);
                    reportBatch.Clear(); resultBatch.Clear();
                }
            }
        }
        if (reportBatch.Count > 0) await BulkAddAsync(db, reportBatch, 2000);
        if (resultBatch.Count > 0) await BulkAddAsync(db, resultBatch, 20000);

        db.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    private static (string code, string name, string units, decimal value, decimal refLow, decimal refHigh) GetCBCAnalyte(int index, Random rand)
    {
        var cbcTests = new[] {
            ("WBC", "White Blood Cell Count", "cells/μL", 5000m, 15000m),
            ("RBC", "Red Blood Cell Count", "cells/μL", 5.0m, 7.0m),
            ("HGB", "Hemoglobin", "g/dL", 10.0m, 18.0m),
            ("HCT", "Hematocrit", "%", 35.0m, 55.0m),
            ("MCV", "Mean Corpuscular Volume", "fL", 60.0m, 77.0m),
            ("MCH", "Mean Corpuscular Hemoglobin", "pg", 19.0m, 25.0m),
            ("MCHC", "Mean Corpuscular Hemoglobin Concentration", "g/dL", 32.0m, 36.0m),
            ("PLT", "Platelet Count", "cells/μL", 150000m, 500000m),
            ("NEU%", "Neutrophil Percentage", "%", 55.0m, 75.0m),
            ("LYM%", "Lymphocyte Percentage", "%", 12.0m, 30.0m),
            ("MON%", "Monocyte Percentage", "%", 3.0m, 10.0m),
            ("EOS%", "Eosinophil Percentage", "%", 2.0m, 10.0m),
            ("BAS%", "Basophil Percentage", "%", 0.0m, 1.0m),
            ("RDW", "Red Cell Distribution Width", "%", 11.5m, 15.5m),
            ("MPV", "Mean Platelet Volume", "fL", 8.0m, 12.0m),
            ("RETIC", "Reticulocyte Count", "%", 0.5m, 2.5m),
            ("NRBC", "Nucleated RBC", "/100WBC", 0.0m, 2.0m),
            ("PCT", "Plateletcrit", "%", 0.15m, 0.35m)
        };

        if (index >= cbcTests.Length) 
        {
            var test = cbcTests[index % cbcTests.Length];
            return (test.Item1, test.Item2, test.Item3, 
                Math.Round((decimal)(rand.NextDouble() * (double)(test.Item5 - test.Item4)) + test.Item4, 2), 
                test.Item4, test.Item5);
        }

        var selected = cbcTests[index];
        var value = Math.Round((decimal)(rand.NextDouble() * (double)(selected.Item5 - selected.Item4) * 1.3 + (double)selected.Item4 * 0.85), 2);
        return (selected.Item1, selected.Item2, selected.Item3, value, selected.Item4, selected.Item5);
    }

    private static (string code, string name, string units, decimal value, decimal refLow, decimal refHigh) GetChemistryAnalyte(int index, Random rand)
    {
        var chemTests = new[] {
            ("BUN", "Blood Urea Nitrogen", "mg/dL", 10.0m, 25.0m),
            ("CREA", "Creatinine", "mg/dL", 0.5m, 1.5m),
            ("GLU", "Glucose", "mg/dL", 70.0m, 140.0m),
            ("ALT", "Alanine Aminotransferase", "U/L", 10.0m, 80.0m),
            ("AST", "Aspartate Aminotransferase", "U/L", 10.0m, 60.0m),
            ("ALP", "Alkaline Phosphatase", "U/L", 20.0m, 150.0m),
            ("TBIL", "Total Bilirubin", "mg/dL", 0.1m, 0.5m),
            ("TP", "Total Protein", "g/dL", 5.4m, 7.5m),
            ("ALB", "Albumin", "g/dL", 2.3m, 4.0m),
            ("CHOL", "Total Cholesterol", "mg/dL", 135.0m, 270.0m),
            ("TG", "Triglycerides", "mg/dL", 29.0m, 291.0m),
            ("CA", "Calcium", "mg/dL", 9.0m, 11.2m),
            ("PHOS", "Phosphorus", "mg/dL", 2.5m, 6.0m),
            ("NA", "Sodium", "mmol/L", 145.0m, 157.0m),
            ("K", "Potassium", "mmol/L", 3.5m, 5.8m),
            ("CL", "Chloride", "mmol/L", 105.0m, 118.0m),
            ("CO2", "Carbon Dioxide", "mmol/L", 17.0m, 25.0m),
            ("CK", "Creatine Kinase", "U/L", 10.0m, 200.0m)
        };

        if (index >= chemTests.Length)
        {
            var test = chemTests[index % chemTests.Length];
            return (test.Item1, test.Item2, test.Item3,
                Math.Round((decimal)(rand.NextDouble() * (double)(test.Item5 - test.Item4)) + test.Item4, 2),
                test.Item4, test.Item5);
        }

        var selected = chemTests[index];
        var value = Math.Round((decimal)(rand.NextDouble() * (double)(selected.Item5 - selected.Item4) * 1.3 + (double)selected.Item4 * 0.85), 2);
        return (selected.Item1, selected.Item2, selected.Item3, value, selected.Item4, selected.Item5);
    }

    private static (string code, string name, string units, decimal value, decimal refLow, decimal refHigh) GetUrinalysisAnalyte(int index, Random rand)
    {
        var uaTests = new[] {
            ("SG", "Specific Gravity", "", 1.015m, 1.045m),
            ("PH", "pH", "", 5.5m, 7.5m),
            ("PROT", "Protein", "mg/dL", 0.0m, 30.0m),
            ("GLU", "Glucose", "mg/dL", 0.0m, 15.0m),
            ("KET", "Ketones", "mg/dL", 0.0m, 5.0m),
            ("BIL", "Bilirubin", "mg/dL", 0.0m, 0.2m),
            ("URO", "Urobilinogen", "mg/dL", 0.1m, 1.0m),
            ("NIT", "Nitrite", "neg/pos", 0.0m, 0.0m),
            ("WBC", "White Blood Cells", "/hpf", 0.0m, 5.0m),
            ("RBC", "Red Blood Cells", "/hpf", 0.0m, 3.0m),
            ("EPI", "Epithelial Cells", "/hpf", 0.0m, 5.0m),
            ("CAST", "Casts", "/lpf", 0.0m, 2.0m)
        };

        if (index >= uaTests.Length)
        {
            var test = uaTests[index % uaTests.Length];
            return (test.Item1, test.Item2, test.Item3,
                Math.Round((decimal)(rand.NextDouble() * (double)(test.Item5 - test.Item4)) + test.Item4, 2),
                test.Item4, test.Item5);
        }

        var selected = uaTests[index];
        var value = Math.Round((decimal)(rand.NextDouble() * (double)(selected.Item5 - selected.Item4) * 1.2 + (double)selected.Item4 * 0.9), 2);
        return (selected.Item1, selected.Item2, selected.Item3, value, selected.Item4, selected.Item5);
    }

    private static (string code, string name, string units, decimal value, decimal refLow, decimal refHigh) GetThyroidAnalyte(int index, Random rand)
    {
        var thyroidTests = new[] {
            ("T4", "Thyroxine", "μg/dL", 2.0m, 4.5m),
            ("TSH", "Thyroid Stimulating Hormone", "mU/L", 0.1m, 0.6m),
            ("T3", "Triiodothyronine", "ng/dL", 100.0m, 200.0m)
        };

        if (index >= thyroidTests.Length)
        {
            var test = thyroidTests[index % thyroidTests.Length];
            return (test.Item1, test.Item2, test.Item3,
                Math.Round((decimal)(rand.NextDouble() * (double)(test.Item5 - test.Item4)) + test.Item4, 2),
                test.Item4, test.Item5);
        }

        var selected = thyroidTests[index];
        var value = Math.Round((decimal)(rand.NextDouble() * (double)(selected.Item5 - selected.Item4) * 1.4 + (double)selected.Item4 * 0.8), 2);
        return (selected.Item1, selected.Item2, selected.Item3, value, selected.Item4, selected.Item5);
    }

    private static async Task BulkAddAsync<TEntity>(CareflowDb db, List<TEntity> list, int batch)
        where TEntity : class
    {
        for (int i = 0; i < list.Count; i += batch)
        {
            var slice = list.Skip(i).Take(batch).ToList();
            db.AddRange(slice);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }
    }
}