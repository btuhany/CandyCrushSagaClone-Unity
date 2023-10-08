using UnityEngine;
using Tools;

namespace Core
{
    public class MatchablePool : ObjectPool<Matchable>
    {
        [SerializeField] private MatchableVariant[] _matchableVariants;


        [Space]
        [NonReorderable]
        [Header("Red, Blue, Green, Purple, Orange, Yellow\r\nFrom lower to higher values in range of 0, 1")]
        [SerializeField] 
        private float[] _colorPossibilityNodes =  new float[5];

        [NonReorderable]
        [Header("Normal, HorizontalExplode, VerticalExplode, AreaExplode, ColorExplode\r\nFrom lower to higher values in range of 0, 1")]
        [SerializeField]
        private float[] _typePossibilityNodes = new float[5];
        
        public Matchable GetRandomVariantMatchable(bool active)
        {
            Matchable newMatchable = GetObject(active);
            RandomizeVariantOf(newMatchable);
            return newMatchable;
        }
        private void RandomizeVariantOf(Matchable matchableToRandom)
        {
            MatchableColor randomColor;
            MatchableType randomType = GetRandomMatchableType();
            
            if (randomType == MatchableType.ColorExplode)
                randomColor = MatchableColor.None;
            else
                randomColor = GetRandomMatchableColor();

            MatchableVariant randomVariant = null;
            foreach (MatchableVariant variant in _matchableVariants)
            {
                if(variant.color == randomColor && variant.type == randomType)
                {
                    randomVariant = variant;
                }
            }

            if (randomVariant == null)
                Debug.Log("Variant not found");
            
            matchableToRandom.SetVariant(randomVariant);
        }
        public void ChangeToAnotherRandomVariant(Matchable matchableToRandom)
        {
            MatchableVariant currentVariant = matchableToRandom.Variant;
            MatchableColor randomColor;
            MatchableType randomType = GetRandomMatchableType();
            while(randomType == MatchableType.ColorExplode && currentVariant.type == MatchableType.ColorExplode)
            {
                randomType = GetRandomMatchableType();
            }

            if (randomType == MatchableType.ColorExplode)
            {
                randomColor = MatchableColor.None;
            }
            else
            {
                randomColor = GetRandomMatchableColor();
                while(randomColor == currentVariant.color)
                {
                    randomColor = GetRandomMatchableColor();
                }
            }

            MatchableVariant randomVariant = null;
            foreach (MatchableVariant variant in _matchableVariants)
            {
                if (variant.color == randomColor && variant.type == randomType)
                {
                    randomVariant = variant;
                }
            }

            if (randomVariant == null)
                Debug.Log("Variant not found");

            matchableToRandom.SetVariant(randomVariant);
        }

        private MatchableColor GetRandomMatchableColor()
        {
            float randomNumber = Random.Range(0.0f, 1.0f);
            if (randomNumber > _colorPossibilityNodes[4])
                return MatchableColor.Yellow;
            else if (randomNumber > _colorPossibilityNodes[3])
                return MatchableColor.Orange;
            else if (randomNumber > _colorPossibilityNodes[2])
                return MatchableColor.Purple;
            else if (randomNumber > _colorPossibilityNodes[1])
                return MatchableColor.Green;
            else if (randomNumber > _colorPossibilityNodes[0])
                return MatchableColor.Blue;
            else
                return MatchableColor.Red;
        }
        private MatchableType GetRandomMatchableType()
        {
            float randomNumber = Random.Range(0.0f, 1.0f);
            if (randomNumber > _typePossibilityNodes[3])
                return MatchableType.ColorExplode;
            else if (randomNumber > _typePossibilityNodes[2])
                return MatchableType.AreaExplode;
            else if (randomNumber > _typePossibilityNodes[1])
                return MatchableType.VerticalExplode;
            else if (randomNumber > _typePossibilityNodes[0])
                return MatchableType.HorizontalExplode;
            else
                return MatchableType.Normal;
        }
        public bool RollDice(float possibility)
        {
            possibility = Mathf.Clamp01(possibility);
            if (Random.Range(0f, 1f) < 1 * possibility)
                return true;
            return false;
        }
        
    }
}

