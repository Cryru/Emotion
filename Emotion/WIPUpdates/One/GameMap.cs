﻿using Emotion.WIPUpdates.One.Work;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One;

public class GameMap
{
    public IEnumerator LoadRoutine()
    {
        yield break;
    }

    public void AddObject(MapObject obj)
    {

    }

    public void AddAndInitObject(MapObject obj)
    {
        AddObject(obj);
    }
}