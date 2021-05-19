using System.ComponentModel.DataAnnotations.Schema;

namespace OrderSupervisor.Common.Models
{
    public class EnqueueMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        public int RandomNumber { get; set; }
        public string OrderText { get; set; }
    }
}
