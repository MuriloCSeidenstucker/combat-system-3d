using UnityEngine;

public interface IColliderInfo
{
    float Radius { get; set; }
    float Height { get; set; }
    Vector3 Center { get; set; }
    Collider Collider { get; }
}

public class ColliderInfoFactory
{
    public static IColliderInfo NewColliderInfo(Collider collider)
    {
        if (collider.GetType() == typeof(CapsuleCollider))
        {
            return new CapsuleColliderInfo((CapsuleCollider)collider);
        }

        throw new System.Exception("No ColliderInfo implementation for type: " + (collider != null ? collider.name : "NULL"));
    }
}

public class CapsuleColliderInfo : IColliderInfo
{
    public CapsuleColliderInfo(CapsuleCollider inCapsuleCollider)
    {
        capsuleCollider = inCapsuleCollider;
    }

    private CapsuleCollider capsuleCollider;

    public Collider Collider => capsuleCollider;

    public float Radius
    {
        get => capsuleCollider.radius;
        set => capsuleCollider.radius = value;
    }

    public float Height
    {
        get => capsuleCollider.height;
        set => capsuleCollider.height = value;
    }

    public Vector3 Center
    {
        get => capsuleCollider.center;
        set => capsuleCollider.center = value;
    }
}
