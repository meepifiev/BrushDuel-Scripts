using System;
using _Project.Application.Configs;
using _Project.Application.Match;
using _Project.Core.Drawing;
using _Project.Core.Match;
using _Project.Core.Scoring;
using VContainer;

namespace _Project.Infrastructure.DI
{
    public static class DrawingSceneFactory
    {
#if UNITY_EDITOR
        private const string EditorFallbackReferencePath = "Assets/_Project/Configs/References/StarryNightConfig.asset";
#endif

        public static ReferenceConfig ResolveReference(IObjectResolver resolver)
        {
            MatchLaunchContext context = resolver.Resolve<MatchLaunchContext>();
            ReferenceConfig reference = context.Reference;

#if UNITY_EDITOR
            if (reference == null)
            {
                reference = UnityEditor.AssetDatabase.LoadAssetAtPath<ReferenceConfig>(EditorFallbackReferencePath);
            }
#endif

            if (reference == null || reference.BakedData == null)
            {
                throw new InvalidOperationException(nameof(reference.BakedData));
            }

            return reference;
        }

        public static CanvasSize CreateCanvasSize(IObjectResolver resolver)
        {
            ReferenceBakedData baked = resolver.Resolve<ReferenceConfig>().BakedData;
            return new CanvasSize(baked.SourceWidth, baked.SourceHeight);
        }

        public static PaletteColor[] CreatePalette(IObjectResolver resolver)
        {
            return resolver.Resolve<ReferenceConfig>().BuildPalette();
        }

        public static ReferenceCanvasMap CreateReferenceCanvasMap(IObjectResolver resolver)
        {
            ReferenceBakedData baked = resolver.Resolve<ReferenceConfig>().BakedData;
            return new ReferenceCanvasMap(resolver.Resolve<CanvasSize>(), baked.CanvasIndices);
        }

        public static IScoreCalculator ResolveScoreCalculator(IObjectResolver resolver)
        {
            MatchLaunchContext context = resolver.Resolve<MatchLaunchContext>();

            if (context.GameMode == GameMode.Masterstroke)
            {
                return resolver.Resolve<MasterstrokeScoreCalculator>();
            }

            return resolver.Resolve<WildstrokeScoreCalculator>();
        }
    }
}
