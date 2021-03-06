﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    public class Attribute
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("type")]
        public int TypeId { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("source")]
        public string Source { get; set; }
    }
}
