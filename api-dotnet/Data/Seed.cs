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
                Address = $"{i} Main St"
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
                    resultBatch.Add(new LabResult {
                        Report = rpt,
                        AnalyteCode = $"A{it:00}",
                        AnalyteName = $"Analyte-{it:00}",
                        ValueNumeric = Math.Round((decimal)(rand.NextDouble() * 100), 2),
                        Units = "U/L",
                        RefLow = 10, RefHigh = 90,
                        Flag = null
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