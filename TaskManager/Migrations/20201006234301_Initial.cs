using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(maxLength: 30, nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    LastEditedTime = table.Column<DateTime>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    Color = table.Column<string>(nullable: true),
                    Tag_Color = table.Column<string>(nullable: true),
                    TaskId = table.Column<int>(nullable: true),
                    Note = table.Column<string>(maxLength: 200, nullable: true),
                    HasToBeDoneTime = table.Column<DateTime>(nullable: true),
                    CategoryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entities_Entities_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entities_Entities_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskTags",
                columns: table => new
                {
                    TaskId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTags", x => new { x.TaskId, x.TagId });
                    table.ForeignKey(
                        name: "FK_TaskTags_Entities_TagId",
                        column: x => x.TagId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TaskTags_Entities_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.Sql(@"CREATE TRIGGER [DELETE_Tag]
                                   ON dbo.[Entities]
                                   INSTEAD OF DELETE
                                   AS
                                BEGIN
                                    DECLARE @DeletedId INT;
                                    DECLARE @Discriminator NVARCHAR (MAX);

                                    DECLARE CUR_Deleted CURSOR FAST_FORWARD FOR
                                    SELECT Id, Discriminator FROM deleted;
                                        
                                     OPEN CUR_Deleted
                                     FETCH NEXT FROM CUR_Deleted INTO @DeletedId, @Discriminator;   

                                     WHILE @@FETCH_STATUS = 0
                                        BEGIN
                                          IF @Discriminator = 'Task'
                                          BEGIN
                                            DELETE FROM[TaskTags] WHERE TaskId IN(@DeletedId)
                                          END

                                          IF @Discriminator = 'Tag'
                                          BEGIN
                                            DELETE FROM[TaskTags] WHERE TagId IN(@DeletedId)
                                          END
                                          
                                          DELETE FROM [Entities] WHERE Id IN(@DeletedId)
                                        
                                          FETCH NEXT FROM CUR_Deleted INTO @DeletedId, @Discriminator;     
                                      END;
                                    CLOSE CUR_Deleted;
                                    DEALLOCATE CUR_Deleted;
                                    
                                END");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_TaskId",
                table: "Entities",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_CategoryId",
                table: "Entities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTags_TagId",
                table: "TaskTags",
                column: "TagId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskTags");

            migrationBuilder.DropTable(
                name: "Entities");
        }
    }
}
