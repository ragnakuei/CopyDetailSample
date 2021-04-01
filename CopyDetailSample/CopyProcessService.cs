using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CopyDetailSample
{
    public class CopyProcessService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;

        private readonly string _connectionString;

        public CopyProcessService(IConfiguration           configuration,
                                  IHostApplicationLifetime appLifetime)
        {
            _appLifetime      = appLifetime;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var sourceGuid = new Guid("69687EF9-C9FF-4609-B1E8-D383CB3C5DDD");

            var boxDto = await GetBoxDtoAsync(sourceGuid);

            ReGuid(boxDto);

            SaveCopied(boxDto);

            _appLifetime.StopApplication();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }

        private async Task<BoxDto> GetBoxDtoAsync(Guid sourceGuid)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
SELECT [c1].*
FROM [dbo].[Category1] [c1]
WHERE [c1].[Guid] = @Guid

SELECT [c2].*
FROM [dbo].[Category2] [c2]
    JOIN [dbo].[Category1] [c1]
         ON [c2].[Category1Guid] = [c1].[Guid]
WHERE [c1].[Guid] = @Guid

SELECT [c3].*
FROM [dbo].[Category3] [c3]
    JOIN [dbo].[Category2] [c2]
         ON [c3].[Category2Guid] = [c2].[Guid]
    JOIN [dbo].[Category1] [c1]
         ON [c2].[Category1Guid] = [c1].[Guid]
WHERE [c1].[Guid] = @Guid
                ";


                var param = new DynamicParameters();
                param.Add("Guid", sourceGuid, DbType.Guid);

                var reader = await conn.QueryMultipleAsync(sql, param);

                var boxDto = new BoxDto();

                boxDto.Category1  = reader.ReadFirstOrDefault<Category1>();
                boxDto.Category2s = reader.Read<Category2>();
                boxDto.Category3s = reader.Read<Category3>();

                return boxDto;
            }
        }

        private void ReGuid(BoxDto boxDto)
        {
            var newGuid = Guid.NewGuid();

            // Category1
            boxDto.Category1.Guid = newGuid;

            // Category2
            boxDto.Category2s
                  .ForEach(c2 =>
                           {
                               c2.Category1Guid = newGuid;

                               var c2NewGuid = Guid.NewGuid();

                               // Category3
                               boxDto.Category3s
                                     .Where(c3 => c3.Category2Guid == c2.Guid)
                                     .ForEach(c3 =>
                                              {
                                                  c3.Guid = Guid.NewGuid();

                                                  c3.Category2Guid = c2NewGuid;
                                              });

                               c2.Guid = c2NewGuid;
                           });
        }

        private void SaveCopied(BoxDto boxDto)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        var sql = @"
INSERT INTO [dbo].[Category1]([Guid], [Name])
VALUES (@Guid, @Name)
";
                        conn.Execute(sql, boxDto.Category1, trans);

                        sql = @"
INSERT INTO [dbo].[Category2]([Guid], [Name], [Category1Guid])
VALUES (@Guid, @Name, @Category1Guid)
";
                        boxDto.Category2s
                              .ForEach(c2 => conn.Execute(sql, c2, trans));

                        sql = @"
INSERT INTO [dbo].[Category3]([Guid], [Name], [Category2Guid])
VALUES (@Guid, @Name, @Category2Guid)
";
                        boxDto.Category3s
                              .ForEach(c3 => conn.Execute(sql, c3, trans));

                        trans.Commit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine(e?.StackTrace);
                        Console.WriteLine(e?.Message);

                        trans.Rollback();
                    }
                }

                conn.Close();
            }
        }
    }
}
