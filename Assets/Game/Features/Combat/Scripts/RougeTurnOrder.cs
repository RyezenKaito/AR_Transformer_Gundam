using System;
using System.Collections.Generic;

namespace GundamGame.CombatDemo
{
    /// <summary>Action values in simulation time, not wall-clock seconds.</summary>
    public sealed class RougeTurnOrder
    {
        readonly List<DemoFighter> actors=new List<DemoFighter>();
        readonly Dictionary<DemoFighter,double> next=new Dictionary<DemoFighter,double>();
        double now;
        public void Reset(DemoFighter player,DemoFighter[] enemies)
        {
            actors.Clear();next.Clear();now=0;
            actors.Add(player);foreach(var actor in enemies)if(actor!=null)actors.Add(actor);
            foreach(var actor in actors)next[actor]=10000d/Math.Max(1,actor.speed);
        }
        public DemoFighter TakeNext()
        {
            DemoFighter choice=null;double best=double.MaxValue;
            // Registration order breaks exact ties deterministically, player first.
            foreach(var actor in actors)if(actor!=null && actor.IsAlive && next[actor]<best){choice=actor;best=next[actor];}
            if(choice==null)return null;
            now=best;next[choice]=now+10000d/Math.Max(1,choice.speed);return choice;
        }
        public void Preview(List<DemoFighter> result,int count,DemoFighter current)
        {
            result.Clear();if(current!=null && current.IsAlive)result.Add(current);
            var copy=new Dictionary<DemoFighter,double>(next);
            while(result.Count<count)
            {
                DemoFighter choice=null;double best=double.MaxValue;
                foreach(var actor in actors)if(actor!=null && actor.IsAlive && copy[actor]<best){choice=actor;best=copy[actor];}
                if(choice==null)break;
                result.Add(choice);copy[choice]=best+10000d/Math.Max(1,choice.speed);
            }
        }
    }
}
