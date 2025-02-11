using System.Collections.Generic;
using UnityEngine;

public class Quadtree
{
    private const int MAX_PARTICLES = 10; // Adjust for performance
    private Rect bounds;
    private List<Particle> particles;
    private Quadtree[] children;
    private bool isDivided;

    public Quadtree(Rect bounds)
    {
        this.bounds = bounds;
        particles = new List<Particle>();
        isDivided = false;
    }

    public void Insert(Particle particle)
    {
        if (!bounds.Contains(particle.transform.position)) return;

        if (particles.Count < MAX_PARTICLES && !isDivided)
        {
            particles.Add(particle);
        }
        else
        {
            if (!isDivided) Subdivide();
            foreach (var child in children)
            {
                child.Insert(particle);
            }
        }
    }

    private void Subdivide()
    {
        float halfWidth = bounds.width / 2f;
        float halfHeight = bounds.height / 2f;
        Vector2 pos = bounds.position;

        children = new Quadtree[]
        {
            new Quadtree(new Rect(pos.x, pos.y, halfWidth, halfHeight)), // Top-left
            new Quadtree(new Rect(pos.x + halfWidth, pos.y, halfWidth, halfHeight)), // Top-right
            new Quadtree(new Rect(pos.x, pos.y + halfHeight, halfWidth, halfHeight)), // Bottom-left
            new Quadtree(new Rect(pos.x + halfWidth, pos.y + halfHeight, halfWidth, halfHeight)) // Bottom-right
        };
        isDivided = true;
    }

    public List<Particle> QueryRange(Rect range, List<Particle> found = null)
    {
        if (found == null) found = new List<Particle>();

        if (!bounds.Overlaps(range)) return found;

        foreach (var p in particles)
        {
            if (range.Contains(p.transform.position))
            {
                found.Add(p);
            }
        }

        if (isDivided)
        {
            foreach (var child in children)
            {
                child.QueryRange(range, found);
            }
        }

        return found;
    }

    public void Clear()
    {
        particles.Clear();
        if (isDivided)
        {
            foreach (var child in children)
            {
                child.Clear();
            }
            isDivided = false;
        }
    }
}
