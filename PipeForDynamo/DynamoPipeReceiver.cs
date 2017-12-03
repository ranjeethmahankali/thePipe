﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Types;
using PipeForDynamo.Converters;

namespace PipeForDynamo
{
    internal class DynamoPipeReceiver : DynamoPipeWrapper, IPipeEmitter
    {
        internal DynamoPipeReceiver(string pipeIdentifier, DynamoPipeConverter converter) : base(pipeIdentifier, converter)
        {
            _pipe.SetEmitter(this);
        }

        public void EmitPipeData(DataNode data)
        {
            List<object> objs = new List<object>();
            foreach (var child in data.ChildrenList)
            {
                objs.Add(_converter.FromPipe<object, IPipeMemberType>(child.Data));
            }
            Data = objs.Count == 1 ? objs[0] : objs;
        }
    }
}