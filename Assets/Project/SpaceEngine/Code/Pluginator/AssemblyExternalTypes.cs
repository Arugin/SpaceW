﻿using System;
using System.Collections;
using System.Collections.Generic;

public class AssemblyExternalTypes : Dictionary<Type, List<Type>>
{
    public AssemblyExternalTypes(Type type, List<Type> value)
    {
        this.Add(type, value);
    }
}