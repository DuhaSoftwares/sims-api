using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Duha.SIMS.DomainModels.Base
{
    public abstract class DModelRoot<T> : DomainModelRoot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public T Id { get; set; }

        public string? CreatedBy { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
