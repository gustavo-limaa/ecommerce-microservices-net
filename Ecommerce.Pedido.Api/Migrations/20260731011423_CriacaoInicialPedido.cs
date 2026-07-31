using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Pedido.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicialPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClienteCpf = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false),
                    Endereco_Bairro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Endereco_Cep = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    Endereco_Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Endereco_Complemento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Endereco_Estado = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Endereco_Logradouro = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Endereco_Numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ItensPedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProdutoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NomeProduto = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PedidoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Moeda = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensPedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensPedido_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ItensPedido_PedidoId",
                table: "ItensPedido",
                column: "PedidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensPedido");

            migrationBuilder.DropTable(
                name: "Pedidos");
        }
    }
}
