using System.ComponentModel.DataAnnotations.Schema;

namespace Poseidon
{
    [Table("test")]
    public class TestEntity
    {
        public int id { get; set; }
        public string type { get; set; }
    }
}