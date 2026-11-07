using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontoVoz.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Destinos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DadosSimulados = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEmUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Linhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Letreiro = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    TerminalPrincipal = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    TerminalSecundario = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Sentido = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DadosSimulados = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEmUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Linhas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Paradas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodigoExterno = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DadosSimulados = table.Column<bool>(type: "INTEGER", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    CriadoEmUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paradas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParadasLinhas",
                columns: table => new
                {
                    ParadaId = table.Column<int>(type: "INTEGER", nullable: false),
                    LinhaId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssociadaEmUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParadasLinhas", x => new { x.ParadaId, x.LinhaId });
                    table.ForeignKey(
                        name: "FK_ParadasLinhas_Linhas_LinhaId",
                        column: x => x.LinhaId,
                        principalTable: "Linhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParadasLinhas_Paradas_ParadaId",
                        column: x => x.ParadaId,
                        principalTable: "Paradas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PercursosEstimados",
                columns: table => new
                {
                    ParadaId = table.Column<int>(type: "INTEGER", nullable: false),
                    LinhaId = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinoId = table.Column<int>(type: "INTEGER", nullable: false),
                    MinutosEstimados = table.Column<int>(type: "INTEGER", nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DadosSimulados = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PercursosEstimados", x => new { x.ParadaId, x.LinhaId, x.DestinoId });
                    table.CheckConstraint("CK_PercursosEstimados_MinutosEstimados", "\"MinutosEstimados\" BETWEEN 1 AND 240");
                    table.ForeignKey(
                        name: "FK_PercursosEstimados_Destinos_DestinoId",
                        column: x => x.DestinoId,
                        principalTable: "Destinos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PercursosEstimados_Linhas_LinhaId",
                        column: x => x.LinhaId,
                        principalTable: "Linhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PercursosEstimados_ParadasLinhas_ParadaId_LinhaId",
                        columns: x => new { x.ParadaId, x.LinhaId },
                        principalTable: "ParadasLinhas",
                        principalColumns: new[] { "ParadaId", "LinhaId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PercursosEstimados_Paradas_ParadaId",
                        column: x => x.ParadaId,
                        principalTable: "Paradas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosPassagem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParadaId = table.Column<int>(type: "INTEGER", nullable: false),
                    LinhaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ObservadaEmUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    RegistradaEmUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    PrefixoVeiculo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TipoPassagem = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DadosSimulados = table.Column<bool>(type: "INTEGER", nullable: true),
                    OrigemRegistro = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    ChaveSeed = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    CriadoEmUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosPassagem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosPassagem_Linhas_LinhaId",
                        column: x => x.LinhaId,
                        principalTable: "Linhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosPassagem_ParadasLinhas_ParadaId_LinhaId",
                        columns: x => new { x.ParadaId, x.LinhaId },
                        principalTable: "ParadasLinhas",
                        principalColumns: new[] { "ParadaId", "LinhaId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosPassagem_Paradas_ParadaId",
                        column: x => x.ParadaId,
                        principalTable: "Paradas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Destinos_Nome",
                table: "Destinos",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Linhas_Codigo",
                table: "Linhas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paradas_CodigoExterno",
                table: "Paradas",
                column: "CodigoExterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParadasLinhas_LinhaId",
                table: "ParadasLinhas",
                column: "LinhaId");

            migrationBuilder.CreateIndex(
                name: "IX_PercursosEstimados_DestinoId",
                table: "PercursosEstimados",
                column: "DestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_PercursosEstimados_LinhaId",
                table: "PercursosEstimados",
                column: "LinhaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosPassagem_ChaveSeed",
                table: "RegistrosPassagem",
                column: "ChaveSeed",
                unique: true,
                filter: "\"ChaveSeed\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosPassagem_LinhaId",
                table: "RegistrosPassagem",
                column: "LinhaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosPassagem_ParadaId_LinhaId_ObservadaEmUtc",
                table: "RegistrosPassagem",
                columns: new[] { "ParadaId", "LinhaId", "ObservadaEmUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PercursosEstimados");

            migrationBuilder.DropTable(
                name: "RegistrosPassagem");

            migrationBuilder.DropTable(
                name: "Destinos");

            migrationBuilder.DropTable(
                name: "ParadasLinhas");

            migrationBuilder.DropTable(
                name: "Linhas");

            migrationBuilder.DropTable(
                name: "Paradas");
        }
    }
}
