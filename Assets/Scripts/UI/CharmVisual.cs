using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CharmVisual : VisualElement
{
    public int index;
    public readonly Charm charm;

    private readonly int m_size = 70 * (Screen.currentResolution.width / 1920);

    public CharmVisual(Charm _charm)
    {
        charm = _charm;

        name = $"{charm.name}";
        style.height = m_size;
        style.width = m_size;
        style.position = Position.Absolute;

        VisualElement ve = new VisualElement();
        ve.style.backgroundImage = new StyleBackground(_charm.sprite);
        ve.style.flexGrow = 1;
        Add(ve);
    }

    public void SetPosition(Vector2 pos)
    {
        style.left = pos.x;
        style.top = pos.y;
    }

    public CharmVisual CloneElement()
    {
        CharmVisual cv_temp = new CharmVisual(charm.CloneCharm());
        return cv_temp;
    }
}
