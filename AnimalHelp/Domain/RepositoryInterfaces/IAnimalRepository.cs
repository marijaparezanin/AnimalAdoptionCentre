﻿using AnimalHelp.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalHelp.Domain.RepositoryInterfaces
{
    public interface IAnimalRepository : IRepository<Animal>
    {
    }
}
