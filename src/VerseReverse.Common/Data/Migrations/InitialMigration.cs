using FluentMigrator;

namespace TodoAPI.Migrations
{
    [Migration(2022110501)]
    public class InitialMigration : Migration
    {
        public override void Up()
        {
            Create.Table("Articles")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Provider").AsString(100).NotNullable()
                .WithColumn("Url").AsString(1000).NotNullable();

            Create.Table("References")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Book").AsInt16().NotNullable()
                .WithColumn("Chapter").AsInt16().NotNullable()
                .WithColumn("Verse").AsInt16().Nullable();

            Create.Table("ArticleXReference")
                .WithColumn("ArticleId").AsInt32().NotNullable()
                .WithColumn("ReferenceId").AsInt32().NotNullable();

            Create.ForeignKey()
                .FromTable("ArticleXReference").ForeignColumn("ArticleId")
                .ToTable("Articles").PrimaryColumn("Id");

            Create.ForeignKey()
                .FromTable("ArticleXReference").ForeignColumn("ReferenceId")
                .ToTable("References").PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.ForeignKey().FromTable("ArticleXReference").ForeignColumn("ArticleId");
            Delete.ForeignKey().FromTable("ArticleXReference").ForeignColumn("ReferenceId");
            Delete.Table("Articles");
            Delete.Table("References");
            Delete.Table("ArticleXReference");
        }
    }
}
