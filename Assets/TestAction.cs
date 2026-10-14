using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "test", story: "[ooga] [booga]", category: "Action", description: "let's find out what happens :)", id: "79313f571b89c8e407329468d435ccc4")]
public partial class TestAction : Action
{
    [SerializeReference] public BlackboardVariable<List<Vector4>> Ooga;
    [SerializeReference] public BlackboardVariable<BehaviorGraph> Booga;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

