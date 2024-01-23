using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaBlaApp.Model
{
    public class dbContext : DbContext
    {
        public dbContext() : base("CasesDB")
        {
        }
        public DbSet<Model.Article> Articles { get; set; }
        public DbSet<Model.Court> Courts { get; set; }
        public DbSet<Model.Case> Cases { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Case>()
            .HasMany(c => c.Articles)
            .WithMany(a => a.Cases)
            .Map(m =>
            {
                m.ToTable("CaseArticle");
                m.MapLeftKey("CaseId");
                m.MapRightKey("ArticleId");
            });
        }
    }
}
