using System;
using Microsoft.Xna.Framework;

namespace GameEngine3D;

public class PhysicsEntity : Entity
{
    public float WalkSpeed { get; set; } = 5f;
    public float JumpVelocity { get; set; } = 6f;
    public float Gravity { get; set; } = 15f;
    public float MaxFallVel { get; set; } = -30f;
    public Vector3 Velocity { get; set; } = Vector3.Zero;
    public bool OnGround { get; set; } = false;

    public Vector3 HitboxSize { get; set; } = new Vector3(0.4f, 1.6f, 0.4f); 

    private Vector3[] _directions =
    {
        Vector3.Forward,
        Vector3.Backward,
        Vector3.Right,
        Vector3.Left
    };

    public PhysicsEntity(Vector3 position, EntityData entityData, World world) : base(position, entityData, world)
    {
        
    }

    public virtual void PrePhysics(float deltaTime)
    {
        
    }

    public virtual void PostPhysics(float deltaTime)
    {
        
    }

    public void Physics(float deltaTime)
    {
        PrePhysics(deltaTime);

        Position += Velocity * deltaTime;

        bool foundGround = false;
        WorldTile groundTile = null;

        // GRAVITY
        if (Velocity.Y <= 0) // do ground check if not jumping
        {
            for (int i = 0; i < 4; i++)
            {
                WorldTile tileUnder = CurrentWorld.GetTileAtWorld(
                    Position + new Vector3(_directions[i].X * HitboxSize.X, -1f, _directions[i].Z * HitboxSize.Z)
                );

                if (tileUnder != null)
                {
                    if (Position.Y < tileUnder.Position.Y + 1.8f) continue;
                    
                    int empty = 0;
                    for (int j = 0; j < MathF.Ceiling(HitboxSize.Y); j++) // check if hitbox.y 
                    {   
                        WorldTile tileUnderAbove = CurrentWorld.GetTileAtWorld(
                            Position + new Vector3(_directions[i].X * HitboxSize.X, 0f + j, _directions[i].Z * HitboxSize.Z)
                        );

                        if (tileUnderAbove != null) continue;
                        empty++;

                        if (empty == MathF.Ceiling(HitboxSize.Y))
                        {
                            foundGround = true;
                            groundTile = tileUnder;    
                            break;
                        }
                    }
                }
            }
        }

        if (foundGround && groundTile != null)
        {
            Position = new Vector3(Position.X, groundTile.Position.Y + 2f, Position.Z);
            Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
            
            OnGround = true;
        }
        else
        {
            Velocity -= new Vector3(0, Gravity * deltaTime, 0);
            if (Velocity.Y < MaxFallVel)
            {
                Velocity = new Vector3(Velocity.X, MaxFallVel, Velocity.Z);
            }
            OnGround = false;
        }

        // HEAD COLLISION
        WorldTile aboveTile = CurrentWorld.GetTileAtWorld(
            Position + new Vector3(0, HitboxSize.Y - 1, 0)
        );

        if (aboveTile != null) // bonk the head
        {
            Position = new Vector3(Position.X, aboveTile.Position.Y - (HitboxSize.Y - 1) - 0.01f, Position.Z);
            Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
        }


        // X-Z COLLISION
        float newX = Position.X;
        float newZ = Position.Z;

        if (MathF.Abs(Velocity.X) > 0f)
        {
            for (int i = 0; i < MathF.Ceiling(HitboxSize.Y); i++)
            {
                WorldTile tileX = CurrentWorld.GetTileAtWorld(
                    Position + new Vector3(HitboxSize.X / 2 * MathF.Sign(Velocity.X), -1 + i, 0)
                ); // topleft is position

                if (tileX != null)
                {
                    if (Velocity.X < 0) // right of the tile
                    {
                        newX = tileX.Position.X + 1f + HitboxSize.X / 2;
                    }
                    else if (Velocity.X > 0) // left of the tile
                    {
                        newX = tileX.Position.X - HitboxSize.X / 2;
                    }
                }
            }

            Velocity = new Vector3(0, Velocity.Y, Velocity.Z);
        }

        Position = new Vector3(newX, Position.Y, Position.Z);

        if (MathF.Abs(Velocity.Z) > 0f)
        {
            for (int i = 0; i < MathF.Ceiling(HitboxSize.Y); i++)
            {
                WorldTile tileZ = CurrentWorld.GetTileAtWorld(
                    Position + new Vector3(0, -1 + i, HitboxSize.Z / 2 * MathF.Sign(Velocity.Z))
                ); // topleft is position

                if (tileZ != null)
                {
                    if (Velocity.Z < 0) // back of the tile
                    {
                        newZ = tileZ.Position.Z + 1f + HitboxSize.Z / 2;
                    }
                    else if (Velocity.Z > 0) // front of the tile
                    {
                        newZ = tileZ.Position.Z - HitboxSize.Z / 2;
                    }
                }
       
            }   
            
            Velocity = new Vector3(Velocity.X, Velocity.Y, 0);  
        }
        
        Position = new Vector3(Position.X, Position.Y, newZ);

        PostPhysics(deltaTime);
    }
}