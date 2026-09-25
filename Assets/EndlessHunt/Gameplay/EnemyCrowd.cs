using System.Collections.Generic;
using UnityEngine;
namespace EndlessHunt
{
    // Bucket lists are reused; each enemy only checks its local neighbourhood.
    public sealed class EnemyCrowd
    {
        readonly Dictionary<Vector2Int, List<SurvivalEnemy>> _cells = new();
        readonly List<List<SurvivalEnemy>> _buckets = new();
        const float CellSize = 1.5f;
        Vector2Int Cell(Vector2 p) => new(Mathf.FloorToInt(p.x / CellSize), Mathf.FloorToInt(p.y / CellSize));
        public void Resolve(List<SurvivalEnemy> enemies, Vector2 player, float radius, float deltaTime)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                _cells.Clear(); int used = 0;
                foreach (var e in enemies)
                {
                    if (!e.Alive) continue;
                    var key = Cell(e.transform.position);
                    if (!_cells.TryGetValue(key, out var bucket))
                    {
                        if (used == _buckets.Count) _buckets.Add(new List<SurvivalEnemy>(12));
                        bucket = _buckets[used++]; bucket.Clear(); _cells.Add(key, bucket);
                    }
                    bucket.Add(e);
                }
                foreach (var e in enemies)
                {
                    if (!e.Alive) continue;
                    var p = (Vector2)e.transform.position; var key = Cell(p); Vector2 push = Vector2.zero;
                    for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
                    {
                        if (!_cells.TryGetValue(key + new Vector2Int(x, y), out var nearby)) continue;
                        foreach (var other in nearby)
                        {
                            if (other == e || !other.Alive) continue;
                            Vector2 d = p - (Vector2)other.transform.position; float distance = d.magnitude;
                            if (distance >= radius * 2) continue;
                            if (distance < .001f)
                            {
                                float angle = (Mathf.Min(e.GetInstanceID(), other.GetInstanceID()) & 1023) * .618f;
                                d = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (e.GetInstanceID() < other.GetInstanceID() ? 1 : -1);
                            }
                            else d /= distance;
                            push += d * (radius * 2 - distance) * .5f;
                        }
                    }
                    p += Vector2.ClampMagnitude(push, 8 * deltaTime);
                    var fromPlayer = p - player;
                    if (fromPlayer.sqrMagnitude < 1.02f * 1.02f)
                        p = player + (fromPlayer.sqrMagnitude > .0001f ? fromPlayer.normalized : Vector2.up) * 1.02f;
                    e.transform.position = new Vector3(p.x, p.y, e.transform.position.z);
                }
            }
        }
    }
}
