using System.IO;
using Unity.VectorGraphics;
using UnityEngine;

public class PanelData : MonoBehaviour
{
    private AnimationData _animationData = new();
    public SVGImage Icon;
	private string svgContent;

    public AnimationData animationData {
		get => _animationData;
        set {
            _animationData = value;

            if(_animationData != null && _animationData.Graphic != null && _animationData.Graphic.Source != "") {
				initSVG();
            }

        }
    }

	
	private VectorUtils.TessellationOptions tessOptions = new VectorUtils.TessellationOptions() {
		StepDistance = 100.0f,
		MaxCordDeviation = 0.5f,
		MaxTanAngleDeviation = 0.1f,
		SamplingStepSize = 0.01f
	};



    private void initSVG() {
		svgContent = File.ReadAllText(_animationData.Graphic.Source);

		var sceneInfo = loadSVG();
		var geoms = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);

		var sprite = VectorUtils.BuildSprite(geoms, 10.0f, VectorUtils.Alignment.Center, Vector2.zero, 128, true);
		Icon.sprite = sprite;
		
	}

	private SVGParser.SceneInfo loadSVG() {
		using (var reader = new StringReader(svgContent)) { 
			return SVGParser.ImportSVG(reader);
		}
	}

}
