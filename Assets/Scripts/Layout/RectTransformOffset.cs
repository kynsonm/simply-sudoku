using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RectTransformOffset
{
    // ----- Set all dimensions ever -----
    
    public static void All(RectTransform rect, float size) {
        Left(rect, size);
        Right(rect, size);
        Top(rect, size);
        Bottom(rect, size);
    }


    // ----- SET HOR OR VERT DIMENSIONS -----

    public static void Horizontal(RectTransform rect, float size) {
        Left(rect, size);
        Right(rect, size);
    }
    public static void Sides(RectTransform rect, float size) {
        Horizontal(rect, size);
    }

    public static void Vertical(RectTransform rect, float size) {
        Top(rect, size);
        Bottom(rect, size);
    }


    // ----- SET EACH SIDE -----

    public static void Left(RectTransform rect, float left) {
        rect.offsetMin = new Vector2(left, rect.offsetMin.y);
    }

    public static void Right(RectTransform rect, float right) {
        rect.offsetMax = new Vector2(-right, rect.offsetMax.y);
    }

    public static void Top(RectTransform rect, float top) {
        rect.offsetMax = new Vector2(rect.offsetMax.x, -top);
    }

    public static void Bottom(RectTransform rect, float bottom) {
        rect.offsetMin = new Vector2(rect.offsetMin.x, bottom);
    }


    // ----- SET DIMENSIONS TO A SIZE -----

    public static void SetToHeight(RectTransform rect, float size) {
        float offset = (rect.rect.height - size) / 2f;
        Vertical(rect, offset);
    }

    public static void SetToWidth(RectTransform rect, float size) {
        float offset = (rect.rect.width - size) / 2f;
        Sides(rect, offset);
    }
}
