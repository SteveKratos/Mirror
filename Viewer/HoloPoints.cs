using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewer
{
    public sealed class HoloPoints
    {
        private static readonly HoloPoints instance = new HoloPoints();

        public Point3D topLeftpt;
        public Point3D topRightpt;
        public Point3D bottomLeftpt;
        public Point3D bottomRightpt;
        
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static HoloPoints()
        {
        }

        private HoloPoints()
        {
        }

        public static HoloPoints Instance
        {
            get
            {
                return instance;
            }
        }

        int screenWidth_= 595;
        int screenHeight_=390;
        float videoHeight_=390;
        int topMargin_=0;

        public float x_;
        public float y_;

        public Point3D GetHoloPoint(float x,float y)
        {
            if(topLeftpt==null)
            {
                return null;
            }

            x_ = x;
            y_ = y;
            // CGPoint farHoloTopLeftPoint = holoPointMapperData_.farTopLeftPoint;

            Point3D point3D = new Point3D();

            point3D.x = get3Dpoint(topLeftpt.x, topRightpt.x, bottomLeftpt.x, bottomRightpt.x);      
            point3D.y = get3Dpoint(topLeftpt.y, topRightpt.y, bottomLeftpt.y, bottomRightpt.y);
            point3D.z = get3Dpoint(topLeftpt.z, topRightpt.z, bottomLeftpt.z, bottomRightpt.z);

            

            return point3D;

        }

        float get3Dpoint(float topLeftPoint,float topRightPoint,float bottomLeftPoint,float bottomRightPoint)
        {
            float nearViewPortXDepthDiff = 0.0f;

            float nearViewPortYDepthDiff = 0.0f;

            float depthDx = 0.0f;

            float depthDY = 0.0f;

            float xDepth = 0.0f;

            float finalDepth = 0.0f;
    
    
            if(topLeftPoint > topRightPoint )
            {
                nearViewPortXDepthDiff = (topLeftPoint - topRightPoint ) * 100.0f;
        
                depthDx = ((nearViewPortXDepthDiff/screenWidth_) * (x_)) / 100.0f;   // 100 for cm to m convertion, 40 = hololens view port width
        
                xDepth = topLeftPoint - depthDx;
            }
            else if(topLeftPoint<topRightPoint )
            {
                nearViewPortXDepthDiff = (topRightPoint - topLeftPoint ) * 100.0f;
        
                depthDx = ((nearViewPortXDepthDiff/screenWidth_) * (x_)) / 100.0f;   // 100 for cm to m convertion, 40 = hololens view port width
        
                xDepth = topLeftPoint + depthDx;
            }
            else
            {
                xDepth = topLeftPoint;
            }
    
    
            if(topLeftPoint > bottomLeftPoint )
            {
                nearViewPortYDepthDiff = (topLeftPoint - bottomLeftPoint ) * 100.0f;
        
                depthDY = ((nearViewPortYDepthDiff/videoHeight_)* (y_ - topMargin_)) / 100.0f; // 100 for cm to m convertion, 22.5 = hololens view port height
        
                finalDepth = xDepth - depthDY;
            }
            else if(topLeftPoint<bottomLeftPoint )
            {
                nearViewPortYDepthDiff = (bottomLeftPoint - topLeftPoint ) * 100.0f;
        
                depthDY = ((nearViewPortYDepthDiff/videoHeight_)* (y_ - topMargin_)) / 100.0f; // 100 for cm to m convertion, 22.5 = hololens view port height
        
                finalDepth = xDepth + depthDY;
            }
            else
            {
                finalDepth = xDepth;
            }
    
    
    
            return finalDepth;
        }
    }

    public class Point3D
    {
        public float x;
        public float y;
        public float z;
    }

}
