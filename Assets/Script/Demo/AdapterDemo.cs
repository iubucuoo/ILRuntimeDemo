using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdapterDemo : CrossBindingAdaptor
{
    public override Type BaseCLRType => throw new NotImplementedException();

    public override Type AdaptorType => throw new NotImplementedException();

    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    {
        throw new NotImplementedException();
    }

}
