using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Model
{
    public class Rate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CurrencyCode { get; set; } 
        public double ExchangeRate { get; set; } 
        public DateTime Timestamp { get; set; } 
    }
                
}
