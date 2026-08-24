using MyShatteredPixelDungeon.scripts.dungeon.geometry;

namespace MyShatteredPixelDungeon.Tests.Dungeon;

public class GeometryTests
{
    [Fact] public void Point_Constructor() { var p = new Point(3, 5); Assert.Equal(3, p.X); Assert.Equal(5, p.Y); }
    [Fact] public void Point_Offset() { var p = new Point(3, 5); p.Offset(2, -1); Assert.Equal(5, p.X); Assert.Equal(4, p.Y); }
    [Fact] public void Point_Equality() { Assert.Equal(new Point(3, 5), new Point(3, 5)); Assert.NotEqual(new Point(3, 5), new Point(5, 3)); }
    [Fact] public void Rect_Constructor() { var r = new Rect(1, 2, 5, 7); Assert.Equal(4, r.Width); Assert.Equal(5, r.Height); }
    [Fact] public void Rect_Contains() { var r = new Rect(0, 0, 10, 10); Assert.True(r.Contains(0, 0)); Assert.False(r.Contains(10, 10)); }
    [Fact] public void Rect_Intersect() { var a = new Rect(0, 0, 6, 6); var b = new Rect(3, 3, 9, 9); var i = a.Intersect(b); Assert.Equal(3, i.Left); Assert.Equal(6, i.Right); }
    [Fact] public void Rect_NoIntersect() { var a = new Rect(0, 0, 5, 5); var b = new Rect(10, 10, 15, 15); Assert.True(a.Intersect(b).IsEmpty); }
    [Fact] public void Rect_IsEmpty() { Assert.True(new Rect(0, 0, 0, 0).IsEmpty); Assert.False(new Rect(0, 0, 5, 5).IsEmpty); }
    [Fact] public void Rect_Resize() { var r = new Rect(0, 0, 0, 0); r.Resize(10, 5); Assert.Equal(10, r.Right); Assert.Equal(5, r.Bottom); }
    [Fact] public void Rect_GetPoints() { Assert.Equal(16, new Rect(0, 0, 3, 3).GetPoints().Count()); }
    [Fact] public void Rect_Shift() { var r = new Rect(1, 2, 5, 7); r.Shift(10, 20); Assert.Equal(11, r.Left); Assert.Equal(27, r.Bottom); }
    [Fact] public void Rect_Set() { var a = new Rect(1, 2, 3, 4); a.Set(new Rect(5, 6, 7, 8)); Assert.Equal(5, a.Left); Assert.Equal(8, a.Bottom); }
    [Fact] public void Rect_Union() { var r = new Rect(0, 0, 5, 5); r.Union(10, 10); Assert.Equal(0, r.Left); Assert.Equal(11, r.Right); }
    [Fact] public void Rect_Shrink() { var s = new Rect(0, 0, 10, 10).Shrink(2); Assert.Equal(2, s.Left); Assert.Equal(8, s.Right); }
}