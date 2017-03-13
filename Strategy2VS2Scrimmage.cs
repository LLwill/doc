using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using xna = Microsoft.Xna.Framework;
using URWPGSim2D.Common;
using URWPGSim2D.StrategyLoader;


namespace URWPGSim2D.Strategy
{
    public class Strategy : MarshalByRefObject, IStrategy
    {


        /// <summary>
        /// 决策类当前对象对应的仿真使命参与队伍的决策数组引用 第一次调用GetDecision时分配空间
        /// </summary>

        //测试用的常亮：sel z
        int sel = 1;
        int z = 4;

        #region 常量部分


        private Decision[] decisions = null;
        private Mission mission = null;
        private int teamId = 0;

        private int count = 0;
        private int minid = 4;
        private int maxid = 0;

        int z1 = 6;
        int z2 = 8;

        private double[] s = new double[9];
        private const int MAX = 1000000;
        private const int MIN = -1000000;
        private const int BALL_IN = 12345;
        private const int BALL_DEAD = 12345;


        private double[] tTable;

        public int flag1 = 0;
        public int flag2 = 0;
        private bool step1 = false;
        private bool step2 = false;
        private bool step3 = false;
        private bool step4 = false;

        private bool startGame = true;
        private bool f1_0 = false;
        private bool f1_1 = false;
        private int fishstep = 27;
        private List<xna.Vector3> balltemp = new List<xna.Vector3>();
        private List<Ball> balls = null;
        private int T = 0;
        private float FPT_0 = 0;
        private xna.Vector3 next = new xna.Vector3(0, 0, 0);
        private bool flag = false;
        private int score = 0;
        private int Times = 0;
        private int fishstep2 = 0;


        int step_2 = 1;
        int step_21 = 1;
        int step_22 = 1;
        int step_1 = 1;
        int step_11 = 1;
        int step_12 = 1;
        

        private int type = -1;
        private float[] balldistance = new float[9];
        private float cnot = 16000;

        private int closeid = -1;
        private bool[] Isin = new bool[9];
        private int enscore = 0;
        private int enemyid = -1;
        #endregion




        #region 常用底层函数
        //=================================================
        //常用底层函数
        //=================================================


        //设置返回一个Decision

        public Decision set_Decision(int tcode, int vcode)
        {
            Decision d = new Decision();
            d.TCode = tcode;
            d.VCode = vcode;

            return d;
        }
        //让鱼静止
        public void jingZhi(int fishID)
        {
            decisions[fishID].TCode = 7;
            decisions[fishID].VCode = 0;
        }
        //左转
        public void turnLeft(int fishID)
        {
            decisions[fishID].TCode = 0;
            decisions[fishID].VCode = 1;
        }
        //右转
        public void turnRight(int fishID)
        {
            decisions[fishID].TCode = 15;
            decisions[fishID].VCode = 1;
        }


        //弧度转为角度 的函数
        public double ang_r2a(double rad)
        {
            return rad * (180 / Math.PI);
        }


        public double distance_p2p(xna.Vector3 a, xna.Vector3 b)
        //计算点到点间的距离
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z));
        }



        // 计算点point 相对鱼的弧度。 
        public double rad_f2p(RoboFish fish, xna.Vector3 point)
        {
            double k_f2p = (fish.PositionMm.Z - point.Z) / (fish.PositionMm.X - point.X);
            double rad_fish = fish.BodyDirectionRad;//鱼头朝向的弧度
            double rad_point; // 鱼到点的 相对坐标系的弧度。
            double rel_rad;  // 鱼和点的相对弧度。



            if (k_f2p >= 0)  //点和鱼的连线斜率是正的，意味着他俩的连线处于右下象限跟左上象限
            {
                rad_point = Math.Atan(k_f2p);        //求出斜率对应的角度大小
                if (fish.PositionMm.Z >= point.Z)   //如果鱼的位置在点的位置之下（向下为变大的方向）
                {
                    rad_point = rad_point - Math.PI;        //那么鱼相对于点的方向应该是个钝角，也就说鱼得逆时针旋转(Math.PI-rad_point)才能正对点
                }
                //如果鱼的位置在点的位置之上，那么就是正常的相对角度
            }
            else  //点和鱼的连线斜率是负的，意味着他俩的连线处于左下象限跟右上象限
            {
                rad_point = Math.Atan(k_f2p);  //求出斜率对应的角度大小
                if (point.Z >= fish.PositionMm.Z)   //如果鱼的位置在点的位置之上（向下为变大的方向）
                {
                    rad_point = rad_point + Math.PI;     //此时，rad_point是负角度，所以鱼需要顺时针旋转（rad_point + Math.PI），也是一个钝角
                }
                //如果鱼的位置在点的位置之下，那么就是正常的相对角度了
            }

            rel_rad = rad_point - rad_fish; // 点到鱼 的弧度  减去 鱼头朝向的弧度 ，得到相对弧度。 



            if (rel_rad > Math.PI) rel_rad = rel_rad - 2 * Math.PI;
            if (rel_rad < -Math.PI) rel_rad = rel_rad + 2 * Math.PI;

            return rel_rad;
        }



        // 计算以A为起点 到B的 两点间连线 相对 坐标系的弧度；
        public double rad_p2p(xna.Vector3 pointA, xna.Vector3 pointB)
        {
            double k_p2p = (pointA.Z - pointB.Z) / (pointA.X - pointB.X);

            double rad_point; // 鱼到点的 相对坐标系的弧度。



            if (k_p2p >= 0)
            {
                rad_point = Math.Atan(k_p2p);
                if (pointA.Z >= pointB.Z) { rad_point = rad_point - Math.PI; }
            }
            else
            {
                rad_point = Math.Atan(k_p2p);
                if (pointB.Z >= pointA.Z) { rad_point = rad_point + Math.PI; }
            }


            if (rad_point > Math.PI) rad_point = rad_point - 2 * Math.PI;
            if (rad_point < -Math.PI) rad_point = rad_point + 2 * Math.PI;

            return rad_point;

        }


        //获取己(my)方球门坐标
        public xna.Vector3 get_Mgoalpos(int teamId, Mission mission)
        {

            Field f = mission.EnvRef.FieldInfo;  // 取得场地对象
            // 根据己方所处半场确定己方球门线中心点的X坐标
            int gpx = (mission.TeamsRef[teamId].Para.MyHalfCourt == HalfCourt.RIGHT) ? f.LeftMm + f.GoalDepthMm : f.RightMm - f.GoalDepthMm;

            xna.Vector3 goalpos = new xna.Vector3(gpx, 0, 0);
            // MessageBox.Show("MY球门" + goalpos.X.ToString());
            return goalpos;
        }


        //获取对(opposition)方球门坐标
        public xna.Vector3 get_Ogoalpos(int teamId, Mission mission)
        {
            Field f = mission.EnvRef.FieldInfo;  // 取得场地对象
            // 根据己方所处半场确定对方球门线中心点的X坐标
            int gpx = (mission.TeamsRef[teamId].Para.MyHalfCourt == HalfCourt.RIGHT) ? f.RightMm - f.GoalDepthMm : f.LeftMm + f.GoalDepthMm;

            xna.Vector3 goalpos = new xna.Vector3(gpx, 0, 0);
            return goalpos;
        }




        //顶球点计算，数学模型--相似三角形
        public xna.Vector3 Shot_Point(RoboFish fish, xna.Vector3 ballpos, xna.Vector3 gatepos)
        {
            float m = 28;       //比球半径58稍大
            float XLen = 0f;    //X坐标偏移量
            float ZLen = 0f;    //Z坐标偏移量
            float diagonalLen = (float)distance_p2p(ballpos, gatepos);

            if (diagonalLen > 0f)
            {
                XLen = m * (ballpos.X - gatepos.X) / diagonalLen;
                ZLen = m * (ballpos.Z - gatepos.Z) / diagonalLen;
            }

            xna.Vector3 shot_point = new xna.Vector3();
            shot_point.X = ballpos.X + XLen;
            shot_point.Z = ballpos.Z + ZLen;
            // MessageBox.Show("球心坐标："+ ballpos.X +"**" +ballpos.Z  + "击球点：" +shot_point.X +"**" + shot_point.Z + "球门:" +gatepos.X.ToString() + "*" + gatepos.Z.ToString()); 



            if (fish.PositionMm.X > ballpos.X && fish.PositionMm.X < gatepos.X && ((fish.PositionMm.X - ballpos.X) > 70))
            {
                shot_point.X = ballpos.X + XLen - 400;
                if (fish.PositionMm.Z > ballpos.Z)
                    shot_point.Z = ballpos.Z + ZLen - 400;
                else
                    shot_point.Z = ballpos.Z + ZLen + 400;

            }

            if (fish.PositionMm.X < ballpos.X && fish.PositionMm.X > gatepos.X && ((fish.PositionMm.X - ballpos.X) < -70))
            {
                shot_point.X = ballpos.X + XLen + 400;
                if (fish.PositionMm.Z > ballpos.Z)
                    shot_point.Z = ballpos.Z + ZLen - 400;
                else
                    shot_point.Z = ballpos.Z + ZLen + 400;

            }

            return shot_point;
        }



        //判断目前的点处于什么象限，右下为第一象限，左下为第二象限，左上为第三象限，右上为第四象限

        public int _inwhichfield(xna.Vector3 point)
        {
            /* if (point.Z + 0.345 * point.X > 800 && (point.X > 0 && point.X <= 1500 && point.Z < 1000))
                 return 1;

             if (point.Z - 0.345 * point.X > 800 && (point.X <= 0 && point.X >= -1500 && point.Z < 1000))
                 return 2;

             if (point.Z + 0.345 * point.X < -800 && (point.X <= 0 && point.X >= -1500 && point.Z > -1000))
                 return 3;

             if (point.Z - 0.345 * point.X < -800 && (point.X > 0 && point.X <= 1500 && point.Z > -1000))
                 return 4;


             //if ((point.X >= 1210 && Math.Abs(point.Z) > 380) || (Math.Abs(point.Z) >= 884) || (Math.Abs(point.Z) > 380 && point.X < -1210))
             //    return 6;

             else
                 return 5;*/

            if (point.Z + 0.345 * point.X > 900 && (point.X > 0))
                return 1;

            if (point.Z - 0.345 * point.X > 900 && (point.X <= 0))
                return 2;

            if (point.Z + 0.345 * point.X < -900 && (point.X <= 0))
                return 3;

            if (point.Z - 0.345 * point.X < -900 && (point.X > 0))
                return 4;


            if ((point.X >= 1210 && Math.Abs(point.Z) > 380) || (Math.Abs(point.Z) >= 884) || (Math.Abs(point.Z) > 380 && point.X < -1210))
            {
                return otherinwich(point);
            }
            return 5;

        }
        public int otherinwich(xna.Vector3 point)
        {
            int v = 0;
            if (point.X >= 0 && point.Z >= 0)
                v = 1;
            if (point.X >= 0 && point.Z < 0)
                v = 4;
            if (point.X < 0 && point.Z < 0)
                v = 3;
            if (point.X < 0 && point.Z > 0)
                v = 2;
            return v;
        }

        public double getJiaJiao(xna.Vector3 fish2ball, xna.Vector3 ball2goal)
        {
            double n = fish2ball.X * ball2goal.X + fish2ball.Z * ball2goal.Z;
            double m = Math.Sqrt(fish2ball.X * fish2ball.X + fish2ball.Z * fish2ball.Z) * Math.Sqrt(ball2goal.X * ball2goal.X + ball2goal.Z * ball2goal.Z);
            return Math.Acos(n / m) * (180 / Math.PI);
        }



        /// <summary>
        /// 扫出对方进的球
        /// </summary>
        /// <param name="fish_no">所使用鱼的编号</param>
        public void KickOffBalls_L(int fish_no)
        {
            xna.Vector3 fishpoint = this.mission.TeamsRef[teamId].Fishes[fish_no].BodyPolygonVertices[1];
            if (fishpoint.X <= -1240)
            {
                if (fishpoint.X > -1300 && fishpoint.Z < -200)
                {
                    GotoPoint(fish_no, new xna.Vector3(fishpoint.X, fishpoint.Y, fishpoint.Z - 10), new xna.Vector3(fishpoint.X, fishpoint.Y, fishpoint.Z - 10));
                }
                else if (fishpoint.Z < -190)
                {
                    GotoPoint(fish_no, new xna.Vector3(fishpoint.X + 20, fishpoint.Y, fishpoint.Z - 10), new xna.Vector3(fishpoint.X + 20, fishpoint.Y, fishpoint.Z - 10));
                }
                else if (fishpoint.X < -1460)
                {
                    GotoPoint(fish_no, new xna.Vector3(fishpoint.X - 10, fishpoint.Y, fishpoint.Z - 20), new xna.Vector3(fishpoint.X - 10, fishpoint.Y, fishpoint.Z - 20));
                }
                else
                {
                    GotoPoint(fish_no, new xna.Vector3(fishpoint.X - 20, fishpoint.Y, fishpoint.Z + 15), new xna.Vector3(fishpoint.X - 20, fishpoint.Y, fishpoint.Z + 15));
                }
            }
            else if (fishpoint.X != -1240 || fishpoint.Z != 210)
            {
                GotoPoint(fish_no, new xna.Vector3(-1240, 0, 200), new xna.Vector3(-1240, 0, 200));
            }


        }
        public void KickOffBalls_L1(int fish_no)
        {
            RoboFish fish = this.mission.TeamsRef[teamId].Fishes[fish_no];
            if (fish.PolygonVertices[0].X > -1200)//维护变量step1234,下次挖球该变量依然有效
            {
                step1 = false;
                step2 = false;
                step3 = false;
                step4 = false;
            }

            if (step1 == false)
            {
                if (fish.PolygonVertices[0].X > -1000 || Math.Abs(fish.PolygonVertices[0].Z) > 380)
                {
                    GotoPoint(fish_no, new xna.Vector3(-1000, 0, -150), new xna.Vector3(-1000, 0, -150));
                }
                if (fish.PolygonVertices[0].X > -1200)
                {
                    GotoPoint(fish_no, new xna.Vector3(-1250, 0, -200), new xna.Vector3(-1250, 0, -200));
                }
                else if (fish.PolygonVertices[0].X > -1300)
                {
                    GotoPoint(fish_no, new xna.Vector3(-1350, 0, -230), new xna.Vector3(-1350, 0, -230));
                }
                else
                {
                    GotoPoint(fish_no, new xna.Vector3(-1500, 0, -230), new xna.Vector3(-1500, 0, -230));
                }
                //------
                if (fish.PolygonVertices[0].X < -1450 && fish.PolygonVertices[0].Z < -170)
                    step1 = true;
            }
            else if (step2 == false)
            {
                if (fish.PolygonVertices[0].Z < 0)
                {
                    GotoPoint(fish_no, new xna.Vector3(fish.PolygonVertices[0].X - 20, 0, fish.PolygonVertices[0].Z + 10), new xna.Vector3(fish.PolygonVertices[0].X - 20, 0, fish.PolygonVertices[0].Z + 10));
                }
                else
                    GotoPoint(fish_no, new xna.Vector3(fish.PolygonVertices[0].X - 200, 0, fish.PolygonVertices[0].Z + 150), new xna.Vector3(fish.PolygonVertices[0].X - 200, 0, fish.PolygonVertices[0].Z + 150));
                //-------
                if (fish.PolygonVertices[0].X < -1450 && fish.PolygonVertices[0].Z > 100)
                    step2 = true;
            }
            else if (step3 == false)
            {
                GotoPoint(fish_no, new xna.Vector3(-1500, 0, 180), new xna.Vector3(-1500, 0, 180));
                if ((Math.Abs(fish.BodyDirectionRad) > Math.PI * 8 / 9) && fish.PolygonVertices[0].X < -1400 && fish.PolygonVertices[0].Z > 180)//
                {
                    step3 = true;
                }
            }
            else if (step4 == false)
            {
                if (fish.PolygonVertices[0].X < -1320)
                    GotoPoint(fish_no, new xna.Vector3(fish.PolygonVertices[0].X, 0, fish.PolygonVertices[0].Z + 50), new xna.Vector3(fish.PolygonVertices[0].X, 0, fish.PolygonVertices[0].Z + 50));
                else if (fish.PolygonVertices[0].X < -1250)
                    GotoPoint(fish_no, new xna.Vector3(fish.PolygonVertices[0].X + 5, 0, fish.PolygonVertices[0].Z + 50), new xna.Vector3(fish.PolygonVertices[0].X + 5, 0, fish.PolygonVertices[0].Z + 50));
                else
                    GotoPoint(fish_no, new xna.Vector3(fish.PolygonVertices[0].X + 20, 0, fish.PolygonVertices[0].Z + 50), new xna.Vector3(fish.PolygonVertices[0].X + 20, 0, fish.PolygonVertices[0].Z + 50));
            }
        }
        #endregion

        #region  常用决策函数
        //===========================
        //常用决策函数
        //===========================

        // 游到指定坐标的实现函数 

        /// <summary>
        /// 游向定点，并转向指定角度（转向角度 需要完善）
        /// </summary>
        /// <param name="fish_no">鱼的编号</param>
        /// <param name="point"> 游向的坐标</param>
        /// <param name="goal"> point 和goal 连线 的弧度 即为 鱼转向的弧度</param>
        public void GotoPoint(int fish_no, xna.Vector3 point, xna.Vector3 goal)
        {

            xna.Vector3 gatepos = goal;  //获取球门坐标


            xna.Vector3 fishpos = mission.TeamsRef[teamId].Fishes[fish_no].PositionMm;  //获取编号为fish_no的鱼的坐标

            xna.Vector3 fishheadpos = mission.TeamsRef[teamId].Fishes[fish_no].PolygonVertices[0];//鱼头的点

            xna.Vector3 ballpos = point;  //获取球的点位


            double bodydirection = mission.TeamsRef[teamId].Fishes[fish_no].BodyDirectionRad;  //当前鱼体的指向，用弧度表示

            double angularvelocity = mission.TeamsRef[teamId].Fishes[fish_no].AngularVelocityRadPs; //当前角速度，rad/s



            double rad_ball2gate = rad_p2p(ballpos, gatepos);   //球与球门的夹角，用弧度表示


            xna.Vector3 shot_point = Shot_Point(mission.TeamsRef[teamId].Fishes[fish_no], point, goal);   //获取顶球点的坐标


            double dist_s2f = distance_p2p(shot_point, mission.TeamsRef[teamId].Fishes[fish_no].PolygonVertices[0]);//得到顶球点跟 鱼头顶部 的距离

            if (dist_s2f < 10) { dist_s2f = rad_ball2gate; } //如果鱼头离球的距离<10，那么


            double rad_f2s = rad_p2p(mission.TeamsRef[teamId].Fishes[fish_no].PositionMm, shot_point);//得到鱼与射门点的相对夹角,弧度制


            double rel_rad_f2s = rad_f2s - mission.TeamsRef[teamId].Fishes[fish_no].BodyDirectionRad; //鱼跟射门点真实弧度为 鱼与射门点的相对夹角减去 鱼的朝向角
            if (rel_rad_f2s > Math.PI) { rel_rad_f2s -= 2 * Math.PI; }
            else if (rel_rad_f2s < -Math.PI) { rel_rad_f2s += 2 * Math.PI; };


            double dist_velocity = dist_s2f / 317.0;
            if (dist_velocity > 2.3) { dist_velocity = 2.3; };

            double gear_need = rel_rad_f2s / dist_velocity;

            int index = 7;

            if (rel_rad_f2s <= 0)
            {
                while ((index > 0) && (tTable[index] > gear_need)) index--;
                if ((angularvelocity / 2.0) < rel_rad_f2s) index = 14;
            }
            else
            {
                while ((index < 15) && (tTable[index] < gear_need)) index++;
                if ((angularvelocity / 2.0) > rel_rad_f2s) index = 2;
            }




            decisions[fish_no].TCode = index;




            if (dist_s2f > 116)
            {

                if (Math.Abs(rel_rad_f2s) < Math.PI / 8)
                {
                    //MessageBox.Show("d3");
                    decisions[fish_no].VCode = 15;
                    // MessageBox.Show("d");
                }
                else
                {
                    // MessageBox.Show("d1");
                    decisions[fish_no].VCode = 1;
                    // MessageBox.Show("d2");
                }
            }

            else if (Math.Abs(rel_rad_f2s) < Math.PI / 18)
            {
                decisions[fish_no].VCode = 6;
            }
            else
            {
                decisions[fish_no].VCode = 1;
            }

            ////////////自己添加的约束条件

            if (mission.TeamsRef[teamId].Fishes[fish_no].VelocityMmPs < 10)
            {
                decisions[fish_no].VCode = 15;
            }



            //处理死球
            //  if()



        }

        public void GotoPoint1(int fish_no, xna.Vector3 point, xna.Vector3 goal)
        {

            xna.Vector3 gatepos = goal;


            xna.Vector3 fishpos = mission.TeamsRef[teamId].Fishes[fish_no].PositionMm;

            xna.Vector3 fishheadpos = mission.TeamsRef[teamId].Fishes[fish_no].PolygonVertices[0];

            xna.Vector3 ballpos = point;


            double bodydirection = mission.TeamsRef[teamId].Fishes[fish_no].BodyDirectionRad;

            double angularvelocity = mission.TeamsRef[teamId].Fishes[fish_no].AngularVelocityRadPs;


            double rad_ball2gate = rad_p2p(ballpos, gatepos);


            //xna.Vector3 shot_point = Shot_Point(mission.TeamsRef[teamId].Fishes[fish_no], point, goal);
            xna.Vector3 shot_point = point;

            double dist_s2f = distance_p2p(shot_point, mission.TeamsRef[teamId].Fishes[fish_no].PolygonVertices[0]);

            if (dist_s2f < 10) { dist_s2f = rad_ball2gate; }


            double rad_f2s = rad_p2p(mission.TeamsRef[teamId].Fishes[fish_no].PositionMm, shot_point);


            double rel_rad_f2s = rad_f2s - mission.TeamsRef[teamId].Fishes[fish_no].BodyDirectionRad;
            if (rel_rad_f2s > Math.PI) { rel_rad_f2s -= 2 * Math.PI; }
            else if (rel_rad_f2s < -Math.PI) { rel_rad_f2s += 2 * Math.PI; };


            double dist_velocity = dist_s2f / 317.0;
            if (dist_velocity > 2.3) { dist_velocity = 2.3; };

            double gear_need = rel_rad_f2s / dist_velocity;

            int index = 7;

            if (rel_rad_f2s <= 0)
            {
                while ((index > 0) && (tTable[index] > gear_need)) index--;
                if ((angularvelocity / 2.0) < rel_rad_f2s) index = 14;
            }
            else
            {
                while ((index < 15) && (tTable[index] < gear_need)) index++;
                if ((angularvelocity / 2.0) > rel_rad_f2s) index = 2;
            }




            decisions[fish_no].TCode = index;




            if (dist_s2f > 116)
            {
                if (dist_s2f > 116 && dist_s2f <= 180)
                {
                    if (Math.Abs(rel_rad_f2s) < Math.PI / 12)
                        decisions[fish_no].VCode = 6;
                    else
                        decisions[fish_no].VCode = 1;
                }

                if (dist_s2f > 180 && dist_s2f <= 300)
                {
                    if (Math.Abs(rel_rad_f2s) < Math.PI / 8)
                        decisions[fish_no].VCode = 10;
                    else
                        decisions[fish_no].VCode = 1;
                }


                if (dist_s2f > 600 && dist_s2f <= 3000)
                {
                    if (Math.Abs(rel_rad_f2s) < Math.PI / 5)
                        decisions[fish_no].VCode = 15;
                    else
                        decisions[fish_no].VCode = 1;
                }

            }

            else if (Math.Abs(rel_rad_f2s) < Math.PI / 18)
            {
                decisions[fish_no].VCode = 6;
            }
            else
            {
                decisions[fish_no].VCode = 1;
            }

            ////////////自己添加的约束条件

            if (mission.TeamsRef[teamId].Fishes[fish_no].VelocityMmPs < 10)
            {
                decisions[fish_no].VCode = 15;
            }



        }

        //判断球是否已经进了   在上半场可以不用把球全部推倒最里边，只要把球运到自己方的球门口就好
        public void ballin()
        {
            for (int i = 0; i < 9; i++)
            {
                xna.Vector3 ballpos = mission.EnvRef.Balls[i].PositionMm;
                if (ballpos.X > 1300 && Math.Abs(ballpos.Z) <= 300)
                    s[i] = BALL_IN;
            }
        }

        public bool is_ballin(int ballid)
        {
            xna.Vector3 ballpos = mission.EnvRef.Balls[ballid].PositionMm;
            int count = numOfGoaled_2();
            if (count < 4)
            {
                if (ballpos.X >= 1400 && Math.Abs(ballpos.Z) <= 300)
                    return true;
            }
            else
            {
                if (ballpos.X >= 1300 && Math.Abs(ballpos.Z) <= 300)
                    return true;

            }
            return false;
        }

        public bool is_ballin_L(int ballid)
        {
            xna.Vector3 ballpos = mission.EnvRef.Balls[ballid].PositionMm;
            if (ballpos.X < -1200 && Math.Abs(ballpos.Z) <= 300)
                return true;
            else return false;
        }

        //得到所有球离球门的距离 (goalpos 是己方或对方球门的坐标)
        public void getallBall_distance()
        {
            count = 0;
            xna.Vector3 goalpos = new xna.Vector3(1500, 0, 0);
            //           double[] s = new double[9];
            for (int i = 0; i < 9; i++)
            {
                xna.Vector3 ballpoint = this.mission.EnvRef.Balls[i].PositionMm;
                if (ballpoint.X > 1268 && Math.Abs(ballpoint.Z) < 300)
                {
                    s[i] = BALL_IN;
                    count++;
                }
                else
                {
                    //  double rad = rad_p2p(ballpoint,goalpos);
                    // float distance = Math.Abs(ballpoint.X - goalpos.X);
                    s[i] = distance_p2p(ballpoint, goalpos);
                }

            }

        }

        public int mindistance_id()
        {
            double max = MAX;
            int id = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] < max && s[i] != BALL_IN && s[i] != BALL_DEAD)
                {
                    max = s[i];
                    id = i;
                }
            }
            return id;

        }


        public int maxdistance_id()
        {
            double min = MIN;
            int id = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] > min && s[i] != BALL_IN && s[i] != BALL_DEAD)
                {
                    min = s[i];
                    id = i;
                }
            }
            return id;

        }



        /**/
        //返回进球数（goalpos己方或对方球门）
        public int numOfGoaled(xna.Vector3 goalpos)
        {
            float _x;
            float _z;
            int count = 0;
            for (int i = 0; i < 9; i++)
            {
                _x = Math.Abs(goalpos.X - this.mission.EnvRef.Balls[i].PositionMm.X);
                _z = Math.Abs(goalpos.Z - this.mission.EnvRef.Balls[i].PositionMm.Z);
                if (_x < 232 && _z < 300)   //还需要减去球的半径
                {
                    count++;
                }
            }
            return count;
        }
        /**/
        //返回进球数（goalpos己方或对方球门）
        public int numOfGoaled_2()
        {

            int count = 0;
            for (int i = 0; i < 9; i++)
            {
                Ball ball = this.mission.EnvRef.Balls[i];

                if (ball.PositionMm.X <= 1068 && ball.PositionMm.Z <= 436 && ball.PositionMm.Z >= -436)
                {
                    count++;
                }

            }
            return count;
        }

        //返回所进球的ID（goalpos己方或对方球门）
        public int[] getGoal_id(xna.Vector3 goalpos)
        {
            int num = numOfGoaled(goalpos);
            if (num > 0)
            {
                int[] ballsId = new int[num];
                int temp = 0;
                for (int i = 0; i < 9; i++)
                {
                    float _x = Math.Abs(goalpos.X - this.mission.EnvRef.Balls[i].PositionMm.X);
                    float _z = Math.Abs(goalpos.Z - this.mission.EnvRef.Balls[i].PositionMm.Z);
                    if (_x < 232 && _z < 300)
                    {
                        ballsId[temp] = i;
                        temp++;
                    }
                }
                return ballsId;
            }
            else
            {
                int[] ballsId = null;
                return ballsId;
            }
        }


        public List<xna.Vector3> workpoint3(Ball ball, xna.Vector3 goalpoint)
        {
            List<xna.Vector3> fv = new List<xna.Vector3>();
            xna.Vector3 v = new xna.Vector3(0, 0, 0);
            xna.Vector3 v2 = new xna.Vector3(0, 0, 0);
            float r = ball.RadiusMm;
            float a = ball.PositionMm.X;
            float b = ball.PositionMm.Z;
            /*   float a = balltemp.X;
               float b = balltemp.Z;*/
            float x = goalpoint.X;
            float y = goalpoint.Z;
            float t = r * r - a * a - b * b;
            float k = (x - a) / (y - b);
            float m = b - k * a;
            float t1 = 1 + k * k;
            float t2 = 2 * m * k - 2 * a - 2 * b * k;
            float c = m * m - 2 * b * m - t;
            float d = t2 * t2 - 4 * t1 * c;
            if (d < 0) d = 0;
            float X = (0 - t2 - (float)Math.Sqrt(d)) / (2 * t1);
            float X2 = (0 - t2 + (float)Math.Sqrt(d)) / (2 * t1);
            float Y = k * X + m;
            float Y2 = k * X2 + m;
            // float R = (float)Math.Sqrt((X - a) * (X - a) + (Y - b) * (Y - b));
            v.X = X;
            v.Z = Y;
            v2.X = X2;
            v2.Z = Y2;
            fv.Add(v);
            fv.Add(v2);
            /* FileStream aFile = new FileStream("G://in.txt", FileMode.OpenOrCreate);//建立一个fileStream对象
             StreamWriter sw = new StreamWriter(aFile);//用FileStream对像实例一个StreamWriter对象
             aFile.Seek(0, SeekOrigin.End);

             sw.WriteLine("______________________________");
             sw.WriteLine("BALL= " + ball.PositionMm + "角速度------>" + ball.AngularVelocityRadPs + "   碰撞状态" + ball.Collision);
             sw.WriteLine("  X=  " + X + "   Y= " + Y + "   R=" + R + "d= " + d);
             sw.WriteLine("鱼头X= " + fish.PolygonVertices[0].X + "  鱼头Y=  " + fish.PolygonVertices[0].Y);
             sw.WriteLine("鱼头刚体X= " + fish.PositionMm.X + "  鱼头刚体Y=  " + fish.PositionMm.Y);
             //sw.WriteLine("角度1--------->"+(Y+752)/(X-916));
             // sw.WriteLine("圆心角度2--------->" + (b + 752) / (a - 916));
             //  sw.WriteLine("圆心角度2--------->" + (b + 752) / (a - 916));
             sw.WriteLine("角度1--------->" + rad_ball(v, new xna.Vector3(916, 0, -752)));
             sw.WriteLine("鱼的角度-------->" + rad_f2p(fish, new xna.Vector3(916, 0, -752)));

             sw.Close();*/



            return fv;
        }
        public double rad_ball(xna.Vector3 point, xna.Vector3 fish)
        {
            /* float x = point2.X - point1.X;
             float z=point2.X-point1.X;
             float k = 0;
             if(x>=0&&z>=0)
             {


             }*/
            double k_f2p = (fish.Z - point.Z) / (fish.X - point.X);

            double rad_point; // 鱼到点的 相对坐标系的弧度。
            double rel_rad;  // 鱼和点的相对弧
            if (k_f2p >= 0)  //点和鱼的连线斜率是正的，意味着他俩的连线处于右下象限跟左上象限
            {
                rad_point = Math.Atan(k_f2p);        //求出斜率对应的角度大小
                if (fish.Z >= point.Z)   //如果鱼的位置在点的位置之下（向下为变大的方向）
                {
                    rad_point = rad_point - Math.PI;        //那么鱼相对于点的方向应该是个钝角，也就说鱼得逆时针旋转(Math.PI-rad_point)才能正对点
                }
                //如果鱼的位置在点的位置之上，那么就是正常的相对角度
            }
            else  //点和鱼的连线斜率是负的，意味着他俩的连线处于左下象限跟右上象限
            {
                rad_point = Math.Atan(k_f2p);  //求出斜率对应的角度大小
                if (point.Z >= fish.Z)   //如果鱼的位置在点的位置之上（向下为变大的方向）
                {
                    rad_point = rad_point + Math.PI;     //此时，rad_point是负角度，所以鱼需要顺时针旋转（rad_point + Math.PI），也是一个钝角
                }
                //如果鱼的位置在点的位置之下，那么就是正常的相对角度了
            }

            rel_rad = rad_point; // 点到鱼 的弧度  减去 鱼头朝向的弧度 ，得到相对弧度。 



            if (rel_rad > Math.PI) rel_rad = rel_rad - 2 * Math.PI;
            if (rel_rad < -Math.PI) rel_rad = rel_rad + 2 * Math.PI;

            return rel_rad;
        }
        #endregion


        #region  策略初始化部分
        public Strategy()
        {
            this.decisions = null;
            this.tTable = new double[] { -0.394f, -0.297f, -0.244f, -0.192f, -0.137f, -0.087f, -0.052f, 0f, 0.052f, 0.087f, 0.137f, 0.192f, 0.244f, 0.297f, 0.349f, 0.349f };


        }

        public void init(Mission mission, int teamId)   //   初始化 函数，在 GetDecision开头调用
        {
            if (this.decisions == null)
            {
                decisions = new Decision[mission.CommonPara.FishCntPerTeam];
            }

            this.mission = mission;
            this.teamId = teamId;


        }
        #endregion






        #region 策略  其他函数


        public override object InitializeLifetimeService()
        {
            //return base.InitializeLifetimeService();   确保对象在下个周期依然存活
            return null; // makes the object live indefinitely
        }
        public string GetTeamName()
        {
            return "DLNU";
        }

        #endregion
        public void fish2()
        {
            int id = close();
            if (id >= 0)
            {
                maxid = id;
                fishstep2 = 4;
            }
//             else
//                 fishstep2 = 0;
            
        }
        public void FoneStep(RoboFish fish0, float X, float Z)
        {
            xna.Vector3 goal = new xna.Vector3(balls[minid].PositionMm.X + X, 0, balls[minid].PositionMm.Z + Z);
            // xna.Vector3 goal = new xna.Vector3(930, 0, 550);
            xna.Vector3 fish2ball = new xna.Vector3(0, 0, 0);
            xna.Vector3 ball2goal = new xna.Vector3(0, 0, 0);
            //             getallBall_distance();
            //             ballin();


            //0号
            if (_inwhichfield(balls[minid].PositionMm) == 1 || _inwhichfield(balls[minid].PositionMm) == 2)
            {
                xna.Vector3 position = new xna.Vector3(0, 0, 0);
                position.X = balls[minid].PositionMm.X + 100;
                position.Z = balls[minid].PositionMm.Z + 100;
                GotoPoint(0, position, position);
                //if (Math.Abs(fish0.PolygonVertices[0].X - position.X) <= 10 && Math.Abs(fish0.PolygonVertices[0].Z - position.Z) <= 10)
                // if (fish0.PolygonVertices[0].Z > position.Z + 58)
                fish2ball.X = balls[minid].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[minid].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[minid].PositionMm.X;
                ball2goal.Z = goal.Z - balls[minid].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    //    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[minid].PositionMm.X;
                    if (balls[minid].PositionMm.Z > 0)
                        position.Z = balls[minid].PositionMm.Z - 100;
                    else
                        position.Z = balls[minid].PositionMm.Z + 100;
                    GotoPoint(0, position, position);


                }
                else if (jiaJiao > 90)
                {
                    //   xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[minid].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[minid].PositionMm.Z + 120;

                        position.X = balls[minid].PositionMm.X - 120;
                    }
                    else
                    {
                        position.Z = balls[minid].PositionMm.Z - 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }


                    GotoPoint(0, position, position);

                }

                else if (jiaJiao > 60)
                {
                    //    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[minid].PositionMm.Z;

                    position.X = balls[minid].PositionMm.X + 100;

                    GotoPoint(0, position, position);

                }

                else
                {
                    GotoPoint(0, balls[minid].PositionMm, goal);

                }


                // GotoPoint(0, position, goal);
            }
            else if (_inwhichfield(balls[minid].PositionMm) == 3 || _inwhichfield(balls[minid].PositionMm) == 4)
            {
                xna.Vector3 position = new xna.Vector3(0, 0, 0);
                position.X = balls[minid].PositionMm.X + 100;
                position.Z = balls[minid].PositionMm.Z - 100;
                GotoPoint(0, position, position);
                fish2ball.X = balls[minid].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[minid].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[minid].PositionMm.X;
                ball2goal.Z = goal.Z - balls[minid].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    //      xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[minid].PositionMm.X;
                    if (balls[minid].PositionMm.Z > 0)
                        position.Z = balls[minid].PositionMm.Z - 100;
                    else
                        position.Z = balls[minid].PositionMm.Z + 100;
                    GotoPoint(0, position, position);


                }
                else if (jiaJiao > 90)
                {
                    //      xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[minid].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[minid].PositionMm.Z + 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }
                    else
                    {
                        position.Z = balls[minid].PositionMm.Z - 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }


                    GotoPoint(0, position, position);

                }

                else if (jiaJiao > 60)
                {
                    //     xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[minid].PositionMm.Z;

                    position.X = balls[minid].PositionMm.X + 100;

                    GotoPoint(0, position, position);

                }

                else
                {
                    GotoPoint(0, balls[minid].PositionMm, goal);

                }

            }

            else
            {
                fish2ball.X = balls[minid].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[minid].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[minid].PositionMm.X;
                ball2goal.Z = goal.Z - balls[minid].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[minid].PositionMm.X;
                    if (balls[minid].PositionMm.Z > 0)
                        position.Z = balls[minid].PositionMm.Z - 100;
                    else
                        position.Z = balls[minid].PositionMm.Z + 100;
                    GotoPoint(0, position, position);


                }
                else if (jiaJiao > 90)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[minid].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[minid].PositionMm.Z + 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }
                    else
                    {
                        position.Z = balls[minid].PositionMm.Z - 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }


                    GotoPoint(0, position, position);

                }

                else if (jiaJiao > 60)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[minid].PositionMm.Z;

                    position.X = balls[minid].PositionMm.X + 100;

                    GotoPoint(0, position, position);

                }

                else
                {
                    GotoPoint(0, balls[minid].PositionMm, goal);

                }
            }
        }
        public void FoneStep_m(RoboFish fish0, xna.Vector3 goal, int n, int fish_no)
        {
            xna.Vector3 fish2ball = new xna.Vector3(0, 0, 0);
            xna.Vector3 ball2goal = new xna.Vector3(0, 0, 0);
            //0号
            if (_inwhichfield(balls[n].PositionMm) == 1 || _inwhichfield(balls[n].PositionMm) == 2)
            {
                xna.Vector3 position = new xna.Vector3(0, 0, 0);
                position.X = balls[n].PositionMm.X + 100;
                position.Z = balls[n].PositionMm.Z + 100;
                GotoPoint(fish_no, position, position);
                //if (Math.Abs(fish0.PolygonVertices[0].X - position.X) <= 10 && Math.Abs(fish0.PolygonVertices[0].Z - position.Z) <= 10)
                // if (fish0.PolygonVertices[0].Z > position.Z + 58)
                fish2ball.X = balls[n].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[n].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[n].PositionMm.X;
                ball2goal.Z = goal.Z - balls[n].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    //    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[n].PositionMm.X;
                    if (balls[n].PositionMm.Z > 0)
                        position.Z = balls[n].PositionMm.Z - 100;
                    else
                        position.Z = balls[n].PositionMm.Z + 100;
                    GotoPoint(fish_no, position, position);


                }
                else if (jiaJiao > 90)
                {
                    //   xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[n].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[n].PositionMm.Z + 120;

                        position.X = balls[n].PositionMm.X - 120;
                    }
                    else
                    {
                        position.Z = balls[n].PositionMm.Z - 120;

                        position.X = balls[n].PositionMm.X + 120;
                    }


                    GotoPoint(fish_no, position, position);

                }

                else if (jiaJiao > 60)
                {
                    //    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[n].PositionMm.Z;

                    position.X = balls[n].PositionMm.X + 100;

                    GotoPoint(fish_no, position, position);

                }

                else
                {
                    GotoPoint(fish_no, balls[n].PositionMm, goal);

                }


                // GotoPoint(0, position, goal);
            }
            else if (_inwhichfield(balls[n].PositionMm) == 3 || _inwhichfield(balls[n].PositionMm) == 4)
            {
                xna.Vector3 position = new xna.Vector3(0, 0, 0);
                position.X = balls[n].PositionMm.X + 100;
                position.Z = balls[n].PositionMm.Z - 100;
                GotoPoint(fish_no, position, position);
                fish2ball.X = balls[n].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[n].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[n].PositionMm.X;
                ball2goal.Z = goal.Z - balls[n].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    //      xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[n].PositionMm.X;
                    if (balls[n].PositionMm.Z > 0)
                        position.Z = balls[n].PositionMm.Z - 100;
                    else
                        position.Z = balls[n].PositionMm.Z + 100;
                    GotoPoint(fish_no, position, position);


                }
                else if (jiaJiao > 90)
                {
                    //      xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[n].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[n].PositionMm.Z + 120;

                        position.X = balls[n].PositionMm.X + 120;
                    }
                    else
                    {
                        position.Z = balls[n].PositionMm.Z - 120;

                        position.X = balls[n].PositionMm.X + 120;
                    }


                    GotoPoint(0, position, position);

                }

                else if (jiaJiao > 60)
                {
                    //     xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[n].PositionMm.Z;

                    position.X = balls[n].PositionMm.X + 100;

                    GotoPoint(fish_no, position, position);

                }

                else
                {
                    GotoPoint(fish_no, balls[n].PositionMm, goal);

                }

            }

            else
            {
                fish2ball.X = balls[n].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[n].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[n].PositionMm.X;
                ball2goal.Z = goal.Z - balls[n].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[n].PositionMm.X;
                    if (balls[n].PositionMm.Z > 0)
                        position.Z = balls[n].PositionMm.Z - 100;
                    else
                        position.Z = balls[n].PositionMm.Z + 100;
                    GotoPoint(fish_no, position, position);


                }
                else if (jiaJiao > 90)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[n].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[n].PositionMm.Z + 120;

                        position.X = balls[n].PositionMm.X + 120;
                    }
                    else
                    {
                        position.Z = balls[n].PositionMm.Z - 120;

                        position.X = balls[n].PositionMm.X + 120;
                    }


                    GotoPoint(fish_no, position, position);

                }

                else if (jiaJiao > 60)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[n].PositionMm.Z;

                    position.X = balls[n].PositionMm.X + 100;

                    GotoPoint(fish_no, position, position);

                }

                else
                {
                    GotoPoint(fish_no, balls[n].PositionMm, goal);

                }
            }


        }
        public void FoneStep0(RoboFish fish0)
        {
            if (Math.Abs(balls[4].PositionMm.X) <= 900&&!GetScore_ball(balls[4]))
            {
                minid = 4;
            }
            else
            {
                minid = Fish1BallChoose();
            }
            if (fish0.PrePositionMm.X < 200)
                GotoPoint(0, new xna.Vector3(250, 0, 350), new xna.Vector3(250, 0, 350));
            else
            {
                if (fish0.BodyDirectionRad > 0)
                {
                    //jingZhi(0);
                    turnRight(0);
                    // turnLeft(0);
                }

            }

            if (fish0.BodyDirectionRad < 0 && fish0.PositionMm.X > 0)
                fishstep = 1;

        }
        public void FoneStep1(RoboFish fish0, List<Ball> balls)
        {

            xna.Vector3 goal = new xna.Vector3(-1502, 0, 350);
            // xna.Vector3 goal = new xna.Vector3(930, 0, 550);
            xna.Vector3 fish2ball = new xna.Vector3(0, 0, 0);
            xna.Vector3 ball2goal = new xna.Vector3(0, 0, 0);
            //             getallBall_distance();
            //             ballin();


            //0号
            if (_inwhichfield(balls[minid].PositionMm) == 1 || _inwhichfield(balls[minid].PositionMm) == 2)
            {
                xna.Vector3 position = new xna.Vector3(0, 0, 0);
                position.X = balls[minid].PositionMm.X + 100;
                position.Z = balls[minid].PositionMm.Z + 100;
                GotoPoint(0, position, position);
                //if (Math.Abs(fish0.PolygonVertices[0].X - position.X) <= 10 && Math.Abs(fish0.PolygonVertices[0].Z - position.Z) <= 10)
                // if (fish0.PolygonVertices[0].Z > position.Z + 58)
                fish2ball.X = balls[minid].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[minid].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[minid].PositionMm.X;
                ball2goal.Z = goal.Z - balls[minid].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    //    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[minid].PositionMm.X;
                    if (balls[minid].PositionMm.Z > 0)
                        position.Z = balls[minid].PositionMm.Z - 100;
                    else
                        position.Z = balls[minid].PositionMm.Z + 100;
                    GotoPoint(0, position, position);


                }
                else if (jiaJiao > 90)
                {
                    //   xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[minid].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[minid].PositionMm.Z + 120;

                        position.X = balls[minid].PositionMm.X - 120;
                    }
                    else
                    {
                        position.Z = balls[minid].PositionMm.Z - 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }


                    GotoPoint(0, position, position);

                }

                else if (jiaJiao > 60)
                {
                    //    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[minid].PositionMm.Z;

                    position.X = balls[minid].PositionMm.X + 100;

                    GotoPoint(0, position, position);

                }

                else
                {
                    GotoPoint(0, balls[minid].PositionMm, goal);

                }


                // GotoPoint(0, position, goal);
            }
            else if (_inwhichfield(balls[minid].PositionMm) == 3 || _inwhichfield(balls[minid].PositionMm) == 4)
            {
                xna.Vector3 position = new xna.Vector3(0, 0, 0);
                position.X = balls[minid].PositionMm.X + 100;
                position.Z = balls[minid].PositionMm.Z - 100;
                GotoPoint(0, position, position);
                fish2ball.X = balls[minid].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[minid].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[minid].PositionMm.X;
                ball2goal.Z = goal.Z - balls[minid].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    //      xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[minid].PositionMm.X;
                    if (balls[minid].PositionMm.Z > 0)
                        position.Z = balls[minid].PositionMm.Z - 100;
                    else
                        position.Z = balls[minid].PositionMm.Z + 100;
                    GotoPoint(0, position, position);


                }
                else if (jiaJiao > 90)
                {
                    //      xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[minid].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[minid].PositionMm.Z + 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }
                    else
                    {
                        position.Z = balls[minid].PositionMm.Z - 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }


                    GotoPoint(0, position, position);

                }

                else if (jiaJiao > 60)
                {
                    //     xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[minid].PositionMm.Z;

                    position.X = balls[minid].PositionMm.X + 100;

                    GotoPoint(0, position, position);

                }

                else
                {
                    GotoPoint(0, balls[minid].PositionMm, goal);

                }

            }

            else
            {
                fish2ball.X = balls[minid].PositionMm.X - fish0.PolygonVertices[0].X;
                fish2ball.Z = balls[minid].PositionMm.Z - fish0.PolygonVertices[0].Z;


                ball2goal.X = goal.X - balls[minid].PositionMm.X;
                ball2goal.Z = goal.Z - balls[minid].PositionMm.Z;
                double jiaJiao = getJiaJiao(fish2ball, ball2goal);
                if (jiaJiao > 150)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.X = balls[minid].PositionMm.X;
                    if (balls[minid].PositionMm.Z > 0)
                        position.Z = balls[minid].PositionMm.Z - 100;
                    else
                        position.Z = balls[minid].PositionMm.Z + 100;
                    GotoPoint(0, position, position);


                }
                else if (jiaJiao > 90)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    if (balls[minid].PositionMm.Z < fish0.PositionMm.Z)
                    {
                        position.Z = balls[minid].PositionMm.Z + 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }
                    else
                    {
                        position.Z = balls[minid].PositionMm.Z - 120;

                        position.X = balls[minid].PositionMm.X + 120;
                    }


                    GotoPoint(0, position, position);

                }

                else if (jiaJiao > 60)
                {
                    xna.Vector3 position = new xna.Vector3(0, 0, 0);
                    position.Z = balls[minid].PositionMm.Z;

                    position.X = balls[minid].PositionMm.X + 100;

                    GotoPoint(0, position, position);

                }

                else
                {
                    GotoPoint(0, balls[minid].PositionMm, goal);

                }
            }

            if ((balls[minid].PositionMm.X - 20) <= -850)
            {



                //decisions[0].TCode = 3;
                fishstep = 2;
                jingZhi(0);

            }

        }
        public void FoneStep2(RoboFish fish0, int m, float fX, float fZ)
        {
            FoneStep(fish0, -100, 100);
            // GotoNextPoint(fish0, m, -150, 100);
            xna.Vector3 x = new xna.Vector3(-936, 0, 548);
            float distance = (float)distance_p2p(fish0.PositionMm, x);
            if (distance <= 100 && fish0.BodyDirectionRad > 0)
                decisions[0].VCode = 2;
            if (balls[m].PositionMm.Z + 10 >= 580)
            {
                fishstep = 3;

            }
            if (fish0.BodyDirectionRad <= -Math.PI / 2)
            {
                fishstep = 0;
            }
        }
        //进球界限下
        public void FoneStep3(RoboFish fish, int m)
        {

            //GotoNextPoint(fish, m, -150, -100);
            FoneStep(fish, -100, -100);

            if (balls[m].PositionMm.X <= -1230)
            {
                //jingZhi(0);
                fishstep = 4;
            }
            if (balls[m].PositionMm.Z >= 740)
            {
                fishstep = 12;
            }


        }
        public void FoneStep4(RoboFish fish, int m)
        {

            // TurnCustom(0, 2, 14);
            turnLeft(0);
            if (fish.BodyDirectionRad <= 3 * Math.PI / 4)
            {
                fishstep = 5;
            }
            //             if (balls[m].PositionMm.Z >= 740)
            //             {
            //                 fishstep = 12;
            //             }
        }
        public void FoneStep5(int fish_no, RoboFish fish, int m)
        {
            GotoPoint(0, new xna.Vector3(-1470, 0, balls[m].PositionMm.Z + 100), new xna.Vector3(-1470, 0, balls[m].PositionMm.Z + 100));
            decisions[0].VCode = 3;
            if (fish.PolygonVertices[0].X <= -1450)
            {
                jingZhi(0);
                fishstep = 6;
            }

        }
        //step6进球处理
        public void FoneStep6(int fish_no, RoboFish fish, int m)
        {
            if (fishstep2 != 6 && fishstep2 != 7)
            {


                xna.Vector3 goal = new xna.Vector3(balls[m].PositionMm.X - 1000, 0, balls[m].PositionMm.Z - 500);
                GotoPoint(0, goal, goal);
                decisions[0].VCode = 5;
                if (balls[m].PositionMm.Z <= 300)
                {
                    fishstep = 7;
                }
            }
            else
            {
                jingZhi(fish_no);
            }

        }
        public void FoneStep7(int fish_no, RoboFish fish, int m)
        {
            TurnCustom(0, 1, 0);
            if (IsOut(fish, m, fish_no))
            {
                fishstep = 9;
            }
            else
            {


                if (fish.BodyDirectionRad <= -3 * Math.PI / 4)
                {
                    //  jingZhi(0);
                    if (GetScore_ball(balls[m]))
                    {
                        score = mission.TeamsRef[teamId].Para.Score;
                        //jingZhi(0);
                        fishstep = 9;
                    }
                    else
                    {
                        fishstep = 8;
                    }

                }
            }
        }
        public void FoneStep8(int fish_no, RoboFish fish, int m)
        {
            if (IsOut(fish, m, fish_no))
            {
                fishstep = 9;
            }
            else
            {


                TurnCustom(0, 1, 15);
                if (fish.BodyDirectionRad >= -Math.PI / 4)
                {
                    //jingZhi(0);
                    if (GetScore(balls[m]))
                    {
                        score = mission.TeamsRef[teamId].Para.Score;
                        jingZhi(0);
                        fishstep = 9;
                    }
                    else
                    {
                        fishstep = 7;
                    }

                }
            }
        }
        //完成一次离开
        public void FoneStep9(int fish_no, RoboFish fish, int m)
        {
            TurnCustom(0, 1, 15);
            if (fish.BodyDirectionRad >= -3 * Math.PI / 4)
            {
                //Straight(fish_no, fish, 10);
                fishstep = 10;
            }

        }
        public void FoneStep27(int fish_no, RoboFish fish, int m)
        {
            if (Math.Abs(balls[4].PositionMm.X) <= 880)
            {
                minid = 4;
            }
            else
            {
                minid = Fish1BallChoose();

            }
            fishstep = 29;
            if (ballinwhich(balls[minid]) >= 2 && ballinwhich(balls[minid]) <= 4)
            {
                fishstep = 28;
            }

        }
        public void FoneStep29(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 x = new xna.Vector3(balls[minid].PositionMm.X + 150, 0, 970);
            GotoPoint(fish_no, x, x);
            if (fish.PositionMm.Z <= 945)
            {
                if (fish.PositionMm.X + 100 < balls[minid].PositionMm.X)
                {
                    fishstep = 31;
                }
                fishstep = 13;
            }

        }
        public void FoneStep31(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 x = new xna.Vector3(-752, 0, 212);
            GotoPoint(fish_no, x, x);
            if (fish.PositionMm.Z <= 380)
                fishstep = 29;
        }
        //鱼一进行补球
        public void FoneStep28(int fish_no, RoboFish fish, int m)
        {
            fishstep = 0;
        }

        public void FoneStep10(int fish_no, RoboFish fish, int m)
        {

            //Fish1GoPointUncondition(0, fish, goal, goal2, 20f, 100f);
            // Straight(fish_no, fish, 8);
            xna.Vector3 x = new xna.Vector3(-1480, 0, -908);
            GotoPoint(fish_no, x, x);
            decisions[fish_no].VCode = 6;
            if (fish.PositionMm.Z <= -700)
            {
                jingZhi(fish_no);
                fishstep = 11;

            }
        }
        public void FoneStep11(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 goal = new xna.Vector3(-580, 0, -940);
            xna.Vector3 goal2 = new xna.Vector3(0, 0, 0);
            GotoPoint(fish_no, goal, goal);
            if (fish.PositionMm.X >= -600)
            {
                fishstep = 27;
            }
        }
        public void FoneStep12(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 goal = new xna.Vector3(balls[m].PositionMm.X + 100, 0, 970);

            GotoPoint(fish_no, goal, goal);
            decisions[fish_no].VCode = 5;
            if (fish.PositionMm.Z >= 880)
            {
                jingZhi(fish_no);
                fishstep = 13;
            }

        }
        //纠错处理下界限
        public void FoneStep13(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 goal = new xna.Vector3(balls[m].PositionMm.X - 500, 0, balls[m].PositionMm.Z + 1000);
            GotoPoint(0, goal, goal);
            // GotoNextPoint(fish,m, 500, 1000);
            decisions[0].VCode = 6;
            if (fish.PositionMm.X + 100 < balls[m].PositionMm.X)
            {
                fishstep = 0;
            }
            if (balls[m].PositionMm.X <= -1300)
            {
                jingZhi(fish_no);
                fishstep = 6;
            }
        }
        public void FoneStep32(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 goal = new xna.Vector3(-416, 0, 456);

            GotoPoint(0, goal, goal);
            // GotoNextPoint(fish,m, 500, 1000);
            decisions[0].VCode = 6;
            if (fish.BodyDirectionRad > 0)
            {
                TurnCustom(fish_no, 1, 0);
            }
            else
            {
                if (fish.PositionMm.Z <= 540)
                {
                    fishstep = 29;
                }
            }
        }
        /************************************************************************/
        /* 
         * 分割线*/
        /************************************************************************/
        public void FoneStep14(int fish_no, RoboFish fish, int m)
        {
            GotoNextPoint(fish, m, -150, -100);
            //FoneStep( fish, 100, -100);
            //   FoneStep_m(fish, new xna.Vector3(920, 0, -830));
            xna.Vector3 x = new xna.Vector3(-936, 0, -548);
            float distance = (float)distance_p2p(fish.PositionMm, x);
            if (distance <= 100 && fish.BodyDirectionRad > 0)
                decisions[0].VCode = 3;

            if (balls[m].PositionMm.Z <= -580)
            {
                fishstep = 15;

            }
            if (fish.BodyDirectionRad >= Math.PI / 4)
            {
                fishstep = 2;
            }
        }
        //进球界限
        public void FoneStep15(int fish_no, RoboFish fish, int m)
        {

            GotoNextPoint(fish, m, -150, 100);
            //  FoneStep(fish0, 100, -100);
            if (balls[m].PositionMm.X <= -1220)
            {
                //jingZhi(0);
                fishstep = 16;
            }
            if (balls[m].PositionMm.Z <= -740)
            {
                fishstep = 23;
            }
        }
        public void FoneStep16(int fish_no, RoboFish fish, int m)
        {
            TurnCustom(0, 2, 2);
            if (fish.BodyDirectionRad <= -Math.PI / 8)
            {
                fishstep = 17;
            }

        }
        public void FoneStep17(int fish_no, RoboFish fish, int m)
        {
            GotoPoint(0, new xna.Vector3(-1470, 0, balls[m].PositionMm.Z - 50), new xna.Vector3(-1470, 0, balls[m].PositionMm.Z - 50));

            decisions[0].VCode = 3;
            if (fish.PolygonVertices[0].X + 15 <= -1450)
            {
                jingZhi(0);
                fishstep = 25;
            }
        }
        public void FoneStep18(int fish_no, RoboFish fish, int m)
        {
            if (IsOut2(fish, m, fish_no))
            {
                fishstep = 20;
            }
            else
            {
                TurnCustom(0, 1, 15);
                if (fish.BodyDirectionRad >= 3 * Math.PI / 4)
                {
                    jingZhi(0);
                    if (GetScore(balls[m]))
                    {
                        score = mission.TeamsRef[teamId].Para.Score;
                        jingZhi(0);
                        fishstep = 20;
                    }
                    else
                    {
                        fishstep = 19;
                    }

                }
            }
        }
        public void FoneStep19(int fish_no, RoboFish fish, int m)
        {
            if (IsOut2(fish, m, fish_no))
            {
                fishstep = 20;
            }
            else
            {
                TurnCustom(0, 1, 0);
                if (fish.BodyDirectionRad <= Math.PI / 2)
                {
                    jingZhi(0);
                    if (GetScore(balls[m]))
                    {
                        score = mission.TeamsRef[teamId].Para.Score;
                        jingZhi(0);
                        fishstep = 20;
                    }
                    else
                    {
                        fishstep = 18;
                    }

                }
            }
        }
        //完成任务离开球门
        public void FoneStep20(int fish_no, RoboFish fish, int m)
        {
            TurnCustom(0, 1, 15);
            if (fish.BodyDirectionRad <= Math.PI / 4)
            {
                //Straight(fish_no, fish, 10);
                fishstep = 21;
            }
        }
        public void FoneStep21(int fish_no, RoboFish fish, int m)
        {
            Straight(fish_no, fish, 8);
            //xna.Vector3 x=new xna.Vector3(-1600,0,-927);
            //GotoPoint(fish_no,x,x );
            if (fish.PositionMm.Z >= -800)
            {
                jingZhi(fish_no);
                fishstep = 22;

            }
        }
        public void FoneStep22(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 goal = new xna.Vector3(-380, 0, 940);
            // xna.Vector3 goal2 = new xna.Vector3(0, 0, 0);
            GotoPoint(fish_no, goal, goal);
            if (fish.PositionMm.X >= -500)
            {
                fishstep = 30;
            }

        }
        public void FoneStep30(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 goal = new xna.Vector3(-580, 0, -940);
            GotoPoint(fish_no, goal, goal);
            if (fish.PositionMm.Z <= -900)
            {
                fishstep = 26;
            }
        }
        public void FoneStep26(int fish_no, RoboFish fish, int m)
        {
            if (ballinwhich_2(balls[4]) != 0)
            {
                minid = 4;
            }
            else
            {
                minid = Fish1BallChoose();
            }
            fishstep = 29;
        }
        //纠错函数1
        public void FoneStep23(int fish_no, RoboFish fish, int m)
        {
            xna.Vector3 goal = new xna.Vector3(balls[m].PositionMm.X + 100, 0, -970);

            GotoPoint(fish_no, goal, goal);
            decisions[fish_no].VCode = 5;
            if (fish.PositionMm.Z <= -880)
            {
                jingZhi(fish_no);
                fishstep = 24;
            }
        }
        public void FoneStep24(int fish_no, RoboFish fish, int m)
        {
            if (fishstep2 != 6 && fishstep2 != 7)
            {
                xna.Vector3 goal = new xna.Vector3(balls[m].PositionMm.X - 500, 0, balls[m].PositionMm.Z - 1000);
                GotoPoint(0, goal, goal);
                // GotoNextPoint(fish,m, 500, 1000);
                decisions[0].VCode = 6;
                if (balls[m].PositionMm.X <= -1260)
                {
                    jingZhi(fish_no);
                    fishstep = 25;
                }
            }
            else
            {
                jingZhi(fish_no);
            }
        }
        //开始进球
        public void FoneStep25(int fish_no, RoboFish fish, int m)
        {
            if (fishstep2 != 6 && fishstep2 != 7)
            {


                xna.Vector3 goal = new xna.Vector3(balls[m].PositionMm.X - 1500, 0, balls[m].PositionMm.Z + 500);
                GotoPoint(0, goal, goal);
                decisions[0].VCode = 5;
                if (balls[m].PositionMm.Z >= -200)
                {

                    fishstep = 18;

                }
            }
            else
            {
                jingZhi(fish_no);
            }

        }
        /************************编号2鱼***************************/
        public void FtwoStep0(int fish_no, RoboFish fish)
        {
            //GotoPoint(fish_no, new xna.Vector3(-250, 0, -300), new xna.Vector3(-250, 0, -300));
            if (enscore > 0)
            {
                fishstep2 = 20;
            }
            else
            {


                if (fish.PositionMm.X <= 200)
                    GotoPoint(fish_no, new xna.Vector3(250, 0, -550), new xna.Vector3(250, 0, -550));
                else
                {
                    fishstep2 = 18;

                }
                int id = close();
                if (id >= 0)
                {
                    maxid = id;
                    fishstep2 = 4;
                }
            }

        }
        //fish2等待激活
        public void FtwoStep18(int fish_no, RoboFish fish)
        {

            if (minid != 4 && ISYellowFish2())
            {
                maxid = 4;

            }
            else
            {
                maxid = Fish1BallChoose_2();
            }
            // print("maxid----->" + maxid);
            fishstep2 = 19;


        }
        public void FtwoStep19(int fish_no, RoboFish fish)
        {
            xna.Vector3 x = new xna.Vector3(-868, 0, 764);
            FoneStep_m(fish, x, maxid, 1);

            if (Isrightfish2(balls[maxid]))
                // jingZhi(fish_no);
                fishstep2 = 0;
        }

        public void FtwoStep1(int fish_no, RoboFish fish)
        {
            jingZhi(fish_no);
            GoOut(fish, fish_no);
            if (Math.Abs(fish.PositionMm.Z) >= 650)
                fishstep2 = 0;

        }
        //辅助进球开始后半段初始
        public void FtwoStep4(int fish_no, RoboFish fish, int i)
        {

            xna.Vector3 x = new xna.Vector3(-1400, 0, 748);
            GotoPoint(fish_no, x, x);
            if (fish.PositionMm.X <= -1300)
            {
                fishstep2 = 5;
            }

        }
        public void FtwoStep5(int fish_no, RoboFish fish, int i)
        {



            xna.Vector3 goal = new xna.Vector3(balls[i].PositionMm.X - 1000, 0, balls[i].PositionMm.Z - 500);
            GotoPoint(fish_no, goal, goal);
            decisions[fish_no].VCode = 5;
            if (balls[i].PositionMm.Z >= fish.PolygonVertices[0].Z)
            {
                fishstep2 = 6;
                // jingZhi(1);
            }
            if (balls[i].PositionMm.Z <= -524)
                fishstep2 = 8;
            if (GetScore(balls[i]))
            {
                fishstep2 = 8;
            }
        }
        //辅助进球过程
        public void FtwoStep6(int fish_no, RoboFish fish, int i)
        {

            TurnCustom(fish_no, 1, 0);
            if (IsOut(fish, i, fish_no))
            {
                fishstep2 = 8;
            }
            else
            {


                if (fish.BodyDirectionRad <= -3 * Math.PI / 4)
                {
                    jingZhi(fish_no);
                    if (GetScore(balls[i]))
                    {
                        score = mission.TeamsRef[teamId].Para.Score;
                        jingZhi(fish_no);
                        fishstep2 = 8;
                    }
                    else
                    {
                        fishstep2 = 7;
                    }

                }
            }
        }
        public void FtwoStep7(int fish_no, RoboFish fish, int i)
        {
            TurnCustom(fish_no, 1, 15);
            if (fish.BodyDirectionRad >= -Math.PI / 4)
            {
                jingZhi(fish_no);
                if (GetScore(balls[i]))
                {
                    score = mission.TeamsRef[teamId].Para.Score;
                    jingZhi(fish_no);
                    fishstep2 = 8;
                }
                else
                {
                    fishstep2 = 6;
                }

            }


        }
        //完成或者异常离开
        public void FtwoStep8(int fish_no, RoboFish fish, int i)
        {
            TurnCustom(fish_no, 1, 15);
            if (fish.BodyDirectionRad >= -3 * Math.PI / 4)
            {
                //Straight(fish_no, fish, 10);
                fishstep2 = 9;
            }


        }
        public void FtwoStep9(int fish_no, RoboFish fish, int i)
        {

            xna.Vector3 x = new xna.Vector3(-1480, 0, -908);
            GotoPoint(fish_no, x, x);
            decisions[fish_no].VCode = 6;
            if (fish.PositionMm.Z <= -700)
            {
                jingZhi(fish_no);
                fishstep2 = 10;

            }
        }
        //到此离开结束等待下一次使用
        public void FtwoStep10(int fish_no, RoboFish fish, int i)
        {
            xna.Vector3 goal = new xna.Vector3(-480, 0, -772);

            GotoPoint(fish_no, goal, goal);
            if (fish.PositionMm.X >= -600)
            {

                fishstep2 = 0;

            }
        }
        /****************************挖对方球***************/
        public void FtwoStep20(int fish_no, RoboFish fish)
        {
            xna.Vector3 x = new xna.Vector3(1400, 0, -664);
            GotoPoint(fish_no, x, x);
            if (fish.PositionMm.X >= 1300)
                fishstep2 = 21;


        }
        public void FtwoStep21(int fish_no, RoboFish fish)
        {
            xna.Vector3 x = new xna.Vector3(1000, 0, 0);
            GotoPoint(fish_no, x, x);
            if (fish.PositionMm.Z >= 0)
            {
                fishstep2 = 22;
            }
            //jingZhi(1);


        }
        public void FtwoStep22(int fish_no, RoboFish fish)
        {


            if (balls[enemyid].PositionMm.Z >= fish.PositionMm.Z)
            {

                xna.Vector3 x = new xna.Vector3(balls[enemyid].PositionMm.X - 500, 0, balls[enemyid].PositionMm.Z + 1000);
                GotoPoint(fish_no, x, x);
                //if (enscore == 0 && balls[enemyid].PositionMm.X >= 1125)
                   // fishstep2 = 1;

            }
            else
            {
                xna.Vector3 x = new xna.Vector3(balls[enemyid].PositionMm.X - 500, 0, balls[enemyid].PositionMm.Z - 1000);
                GotoPoint(fish_no, x, x);
                //if (enscore == 0 && balls[enemyid].PositionMm.X >= 1125)
                   // fishstep2 = 1;
            }
        }
        public void FtwoStep23(int fish_no, RoboFish fish)
        {




        }
        public void FtwoStep26(int fish_no, RoboFish fish)
        {
            xna.Vector3 x = new xna.Vector3(0, 0, 1500);
            GotoPoint(fish_no, x, x);
            decisions[fish_no].VCode = 10;
            if (enscore == 0)
                fishstep2 = 25;

        }
        public void FtwoStep24(int fish_no, RoboFish fish)
        {
            xna.Vector3 x = new xna.Vector3(1024, 0, -160);
            if (fish.BodyDirectionRad <= 3 * Math.PI / 4)
            {
                TurnCustom(fish_no, 1, 15);
            }
            else
            {
                GotoPoint(fish_no, x, x);
                decisions[fish_no].VCode = 6;
                if (fish.PrePositionMm.X <= 1000)
                    fishstep2 = 22;
            }
        }
        //离开归位
        public void FtwoStep25(int fish_no, RoboFish fish)
        {
            jingZhi(fish_no);
            fishstep2 = 1;

        }
        public void Straight(int fish_no, RoboFish fish, int V)
        {
            decisions[fish_no].VCode = V;
            decisions[fish_no].TCode = 7;

        }
        public void Fish1GoPointUncondition(int i, RoboFish fish_no, xna.Vector3 endpoint, xna.Vector3 ballpoint, float anglediff, float threshold)
        {
            Times = 0;
            float endangle = (float)rad_ball(ballpoint, endpoint);
            // float endangle2 = (float)rad_ball( new xna.Vector3(950, 0, -650),ballpoint);
            StrategyHelper.Helpers.PoseToPose(ref decisions[i], fish_no, endpoint, endangle, anglediff, threshold, mission.CommonPara.MsPerCycle, ref Times);
        }
        public void GotoNextPoint(RoboFish fish, int m, float X, float Y)
        {
            xna.Vector3 upGoal = new xna.Vector3(balls[m].PositionMm.X - X, 0, balls[m].PositionMm.Z + Y);
            List<xna.Vector3> t = workpoint3(balls[m], upGoal);
            GotoPoint1(0, t[0], t[0]);

        }
        public void DribbleFish(int i, RoboFish fish_no, xna.Vector3 endpoint, float endangle, float angleTheta1, float angleTheta2, float disThreshold, int VCode1, int VCode2, bool flag)
        {

            StrategyHelper.Helpers.Dribble(ref decisions[i], fish_no, endpoint, endangle, angleTheta1, angleTheta2, disThreshold, VCode1, VCode2, 15, mission.CommonPara.MsPerCycle, false);
        }
        public void TurnCustom(int fishID, int V, int T)
        {
            if (V <= 0)
                V = 1;
            decisions[fishID].TCode = T;
            decisions[fishID].VCode = V;

        }
        public bool IsOut(RoboFish fish, int m, int fish_no)
        {

            float distance = Math.Abs(fish.PositionMm.Z - balls[m].PositionMm.Z);

            if (distance >= 320 || ((balls[m].PositionMm.X < fish.PositionMm.X) && Math.Abs(fish.PositionMm.X - balls[m].PositionMm.X) >= 78) || balls[m].PositionMm.Z - fish.PositionMm.Z >= 300)
            {
                if (enscore == 0)
                {
                    fishstep2 = 4;//召回二号鱼辅助进球
                    type = 0;
                    maxid = minid;
                }

                return true;
            }

            else
                return false;

        }
        public bool Isrightfish2(Ball ball)
        {
            float x = (float)distance_p2p(ball.PositionMm, new xna.Vector3(-780, 0, 944));
            if (x <= 150 || ball.PositionMm.Z >= 776 && ball.PositionMm.X < -380)
                return true;
            else
                return false;

        }
        public bool IsOut2(RoboFish fish, int m, int fish_no)
        {
            //double distance = distance_p2p(fish.PositionMm, balls[m].PositionMm);
            float distance = Math.Abs(fish.PositionMm.Z - balls[m].PositionMm.Z);
            // if (distance >= 330 || balls[m].PositionMm.X > fish.PositionMm.X + 10)
            if (distance >= 330 || ((balls[m].PositionMm.X < fish.PositionMm.X) && Math.Abs(fish.PositionMm.X - balls[m].PositionMm.X) >= 100) || balls[m].PositionMm.Z - fish.PositionMm.Z >= 300)
            {
                if (enscore == 0)
                {
                    fishstep2 = 4;//召回二号鱼辅助进球
                    type = 0;
                    maxid = minid;
                }
                return true;
            }

            else
                return false;

        }
        public void GoOut(RoboFish fish, int fish_no)
        {
            switch (WhereFish2(fish))
            {
                case 1: goto1(fish, fish_no);
                    break;
                case 2: goto2(fish, fish_no);
                    break;
                case 3: goto3(fish, fish_no);
                    break;
                case 4: goto4(fish, fish_no);
                    break;
            }
            //print(WhereFish2(fish) + "");
        }
        public void goto1(RoboFish fish, int fish_no)
        {
            if (fish.BodyDirectionRad > 0)
            {
                TurnCustom(fish_no, 1, 0);
                if (fish.BodyDirectionRad > 0 && fish.BodyDirectionRad > 3 * Math.PI / 4)
                {
                    //Straight(fish_no, fish, 10);
                    //fishstep2 = 2;
                    GotoPoint(fish_no, new xna.Vector3(1400, 0, 790), new xna.Vector3(1400, 0, 790));
                    if (fish.PositionMm.Z >= 760)
                    {
                        fishstep2 = 1;
                        //jingZhi(1);
                    }
                }

            }
            else
            {
                TurnCustom(fish_no, 1, 15);
                if (fish.BodyDirectionRad < 0 && fish.BodyDirectionRad > -Math.PI / 2)
                {
                    //Straight(fish_no, fish, 10);
                    //fishstep2 = 3;
                    GotoPoint(fish_no, new xna.Vector3(1400, 0, -760), new xna.Vector3(1400, 0, -760));
                    if (fish.PositionMm.Z <= -700)
                    {
                        fishstep2 = 1;
                    }
                }
            }


        }
        public void goto2(RoboFish fish, int fish_no)
        {
            if (fish.PositionMm.Z > 0)
            {
                xna.Vector3 x = new xna.Vector3(-1200, 0, 752);
                GotoPoint(fish_no, x, x);


            }
            else
            {
                xna.Vector3 x = new xna.Vector3(-1200, 0, -752);
                GotoPoint(fish_no, x, x);

            }

        }
        public void goto3(RoboFish fish, int fish_no)
        {
            //             if (i == 0)
            //             {
            //                 xna.Vector3 x = new xna.Vector3(0, 0, 752);
            //                 GotoPoint(1, x, x);
            // 
            //             }
            //             else
            //             {
            //                 xna.Vector3 x = new xna.Vector3(0, 0, -752);
            //                 GotoPoint(1, x, x);
            //             }
        }
        public void goto4(RoboFish fish, int fish_no)
        {


        }
        public void GotoNext(RoboFish fish)
        {
            GotoPoint(1, new xna.Vector3(1400, 0, 790), new xna.Vector3(1400, 0, 790));
            if (fish.PositionMm.Z >= 760)
            {
                fishstep2 = 1;
                //jingZhi(1);
            }

        }
        public void GotoNext2(RoboFish fish)
        {
            GotoPoint(1, new xna.Vector3(1400, 0, -760), new xna.Vector3(1400, 0, -760));
            if (fish.PositionMm.Z <= -700)
            {
                fishstep2 = 1;
            }

        }
        public int WhereFish2(RoboFish fish)
        {
            int w = 3;
            float x = fish.PositionMm.X;
            float y = fish.PositionMm.Z;
            if (x >= 964 && Math.Abs(y) <= 530)//右门里面
                w = 1;
            if (x >= 964 && Math.Abs(y) > 530)//右门两侧
                w = 2;
            if (x <= -964 && Math.Abs(y) <= 530)//左门里面
                w = 3;
            if (x <= -964 && Math.Abs(y) > 530)//左门外面
                w = 4;
            return w;
        }
        public int Fish1BallChoose()
        {
            int i = 0, w = 0;
            float min = 16000;
            xna.Vector3 x = new xna.Vector3(-892, 0, 800);
            for (i = 0; i < 9; i++)
            {

                if (ballinwhich(balls[i]) != 0 && !GetScore_ball(balls[i]))
                {
                    balldistance[i] = (float)distance_p2p(x, balls[i].PositionMm);

                }
                else
                {
                    balldistance[i] = cnot;
                }

            }
            for (i = 0; i < 9; i++)
            {
                if (balldistance[i] < min)
                {
                    min = balldistance[i];
                    w = i;
                }
            }
            return w;
        }
        public int Fish1BallChoose_2()
        {
            int i = 0, w = 0;
            float min = 16000;
            xna.Vector3 x = new xna.Vector3(-892, 0, 800);
            for (i = 0; i < 9; i++)
            {
                if (Math.Abs(balls[i].PositionMm.X) <= 800 && i != minid && !Isrightfish2(balls[i]))
                {
                    balldistance[i] = (float)distance_p2p(x, balls[i].PositionMm);

                }
                else
                {
                    balldistance[i] = cnot;
                }
                //print(i+" " + balldistance[i]);
            }
            for (i = 0; i < 9; i++)
            {
                if (balldistance[i] < min)
                {
                    min = balldistance[i];
                    w = i;
                }
            }

            return w;
        }
        public int ballinwhich(Ball ball)
        {
            float x = ball.PositionMm.X;
            float y = ball.PositionMm.Z;
            int w = 0;
            if (Math.Abs(x) <= 880 && y >= 832 && ball.VelocityMmPs <= 13)
                w = 1;
            if (Math.Abs(x) <= 880 && y >= 0 && y < 832 && ball.VelocityMmPs <= 13)
                w = 2;
            if (Math.Abs(x) <= 880 && y < 0 && y >= -832 && ball.VelocityMmPs <= 13)
                w = 3;
            if (Math.Abs(x) <= 880 && y < -832 && ball.VelocityMmPs <= 13)
                w = 4;
            if (x <= 0 && y >= 656 && ball.VelocityMmPs <= 13)
                w = 5;
            return w;
        }
        public int ballinwhich_2(Ball ball)
        {
            float x = ball.PositionMm.X;
            float y = ball.PositionMm.Z;
            int w = 0;
            if (Math.Abs(x) <= 844 && y >= 832 && ball.VelocityMmPs <= 23)
                w = 1;
            if (Math.Abs(x) <= 844 && y >= 0 && y < 832 && ball.VelocityMmPs <= 23)
                w = 2;
            if (Math.Abs(x) <= 844 && y < 0 && y >= -832 && ball.VelocityMmPs <= 23)
                w = 3;
            if (Math.Abs(x) <= 844 && y < -832 && ball.VelocityMmPs <= 23)
                w = 4;
            return w;
        }
        public bool ballinwhich_fish1(Ball ball)
        {
            // float  x = (float)distance_p2p(ball.PositionMm, mission.TeamsRef[teamId].Fishes[0].PolygonVertices[0]);
            if (Math.Abs(ball.PositionMm.X) <= 800 && ball.PositionMm.Z <= 730)
                return true;
            else
                return false;
        }
        //右边边球门
        public bool GetScore_ball_right(Ball ball)
        {
            if ((ball.PositionMm.X >= 1020 && ball.PositionMm.X <= 1068 && ball.PositionMm.Z <= 436 && ball.PositionMm.Z >= -436))
                return true;
            else
                return false;
        }
        //左边球门
        public bool GetScore_ball(Ball ball)
        {
            if ((ball.PositionMm.X <= -1020 && ball.PositionMm.X >= -1068 && ball.PositionMm.Z <= 436 && ball.PositionMm.Z >= -436))
                return true;
            else
                return false;
        }
        //左边球门
        public bool GetScore(Ball ball)
        {
            if ((ball.PositionMm.X <= -1020 && ball.PositionMm.X >= -1068 && ball.PositionMm.Z <= 436 && ball.PositionMm.Z >= -436) || mission.TeamsRef[teamId].Para.Score > score)
                return true;
            else
                return false;
        }
        public bool ISYellowFish2()
        {

            //  print(x + " ");
            if (Math.Abs(balls[4].PositionMm.X) <= 880 && !Isrightfish2(balls[4]))
                return true;
            else
                return false;
        }
        public int close()
        {
            int closeid = -1;
            int i = 0;
            for (i = 0; i < 9; i++)
            {
                if (Math.Abs(balls[i].PositionMm.Z) <= 544 && balls[i].PositionMm.X <= -1128 && balls[i].PositionMm.X >= -1240 && !GetScore_ball(balls[i]))
                {
                    closeid = i;
                }
            }
            return closeid;
        }
       /* public void print(String mesage)
        {


            FileStream aFile = new FileStream("G://data.txt", FileMode.OpenOrCreate);//建立一个fileStream对象
            StreamWriter sw = new StreamWriter(aFile);//用FileStream对像实例一个StreamWriter对象
            aFile.Seek(0, SeekOrigin.End);
            sw.WriteLine("___________________________________________");
            sw.WriteLine(mesage);
            sw.Close();

        }*/
        public void Judge()
        {
            for (int i = 0; i < 9; i++)
            {
                if (GetScore_ball(balls[i]))
                {
                    Isin[i] = true;
                }
                else
                {
                    Isin[i] = false;
                }

            }
           

        }
        public int enemy()
        {
            int id = -1;
            for (int i = 0; i < 9; i++)
            {
                if (GetScore_ball_right(balls[i]))
                {
                    id = i;
                }
            }
            return id;
        }

        public int first_ballId()
        {
            float one = mission.EnvRef.Balls[0].PositionMm.X;
            int first = 0;
            for (int i = 0; i < 9; i++)
            {
                if (mission.EnvRef.Balls[i].PositionMm.X > one)
                {
                    one = mission.EnvRef.Balls[i].PositionMm.X;
                    first = i;
                }
            }
            return first;
        }

        public int first_ballId1()
        {
            float one = mission.EnvRef.Balls[0].PositionMm.X;
            int first = 0;
            for (int i = 0; i < 9; i++)
            {
                if (mission.EnvRef.Balls[i].PositionMm.X < one)
                {
                    one = mission.EnvRef.Balls[i].PositionMm.X;
                    first = i;
                }
            }
            return first;
        }

        public int second_ballId()
        {
            float one = mission.EnvRef.Balls[0].PositionMm.X;
            float two = mission.EnvRef.Balls[0].PositionMm.X;
            int second = 0;
            for (int i = 0; i < 9; i++)
            {
                if (mission.EnvRef.Balls[i].PositionMm.X > one)
                {
                    one = mission.EnvRef.Balls[i].PositionMm.X;

                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (mission.EnvRef.Balls[i].PositionMm.X > two && mission.EnvRef.Balls[i].PositionMm.X != one)
                {
                    two = mission.EnvRef.Balls[i].PositionMm.X;
                    second = i;

                }
            }

            return second;
        }

        public int second_ballId1()
        {
            float one = mission.EnvRef.Balls[0].PositionMm.X;
            float two = mission.EnvRef.Balls[0].PositionMm.X;
            int second = 0;
            for (int i = 0; i < 9; i++)
            {
                if (mission.EnvRef.Balls[i].PositionMm.X < one)
                {
                    one = mission.EnvRef.Balls[i].PositionMm.X;

                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (mission.EnvRef.Balls[i].PositionMm.X < two && mission.EnvRef.Balls[i].PositionMm.X != one)
                {
                    two = mission.EnvRef.Balls[i].PositionMm.X;
                    second = i;

                }
            }

            return second;
        }


        //挖出对方进球
        public void Step1(int fish_no, RoboFish fish)
        {
            xna.Vector3 x = new xna.Vector3(1400, 0, -664);
            GotoPoint(fish_no, x, x);
            
            
                if (fish.PositionMm.X >= 1300)
                    step_2 = 2;
           
            


        }
        public void Step2(int fish_no, RoboFish fish, int z)
        {

            if (balls[z].PositionMm.X > 1150) GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(1500, 0, 1000));
            else
            {
                xna.Vector3 x = new xna.Vector3(1000, 0, 0);
                GotoPoint(fish_no, x, x);
                if (fish.PositionMm.Z >= 0)
                {
                    step_2 = 3;
                }
            }
            
            //jingZhi(1);


        }
        public void Step3(int fish_no, RoboFish fish, int z)
        {


            if (balls[z].PositionMm.Z >= fish.PositionMm.Z)
            {

                xna.Vector3 x = new xna.Vector3(balls[z].PositionMm.X - 500, 0, balls[z].PositionMm.Z + 1000);
                GotoPoint(fish_no, x, x);
                //if (enscore == 0 && balls[enemyid].PositionMm.X >= 1125)
                // fishstep2 = 1;
                if (balls[z].PositionMm.X > 1150) GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(1500, 0, 1000));
            }
            else
            {
                xna.Vector3 x = new xna.Vector3(balls[z].PositionMm.X - 500, 0, balls[z].PositionMm.Z - 1000);
                GotoPoint(fish_no, x, x);
                //if (enscore == 0 && balls[enemyid].PositionMm.X >= 1125)
                // fishstep2 = 1;
                if (balls[z].PositionMm.X > 1150) GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(1500, 0, 1000));
            }
        }


        public void _Step1(int fish_no, RoboFish fish)
        {
            xna.Vector3 x = new xna.Vector3(1400, 0, -664);//位置在左上角
            GotoPoint(fish_no, x, x);
            
            
                if (fish.PositionMm.X >= 1300)
                    step_1 = 2;
           



        }
        public void _Step2(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X > 1150) GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(1500, 0, 1000));
            else
            {
                xna.Vector3 x = new xna.Vector3(1000, 0, 0);
                GotoPoint(fish_no, x, x);
                if (fish.PositionMm.Z >= 0)
                {
                    step_1 = 3;
                }
            }
            
            //jingZhi(1);


        }
        public void _Step3(int fish_no, RoboFish fish, int z)
        {


            if (balls[z].PositionMm.Z >= fish.PositionMm.Z)
            {

                xna.Vector3 x = new xna.Vector3(balls[z].PositionMm.X - 500, 0, balls[z].PositionMm.Z + 1000);
                GotoPoint(fish_no, x, x);
                //if (enscore == 0 && balls[enemyid].PositionMm.X >= 1125)
                // fishstep2 = 1;
                if (balls[z].PositionMm.X > 1150) GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(1500, 0, 1000));
            }
            else
            {
                xna.Vector3 x = new xna.Vector3(balls[z].PositionMm.X - 500, 0, balls[z].PositionMm.Z - 1000);
                GotoPoint(fish_no, x, x);
                //if (enscore == 0 && balls[enemyid].PositionMm.X >= 1125)
                // fishstep2 = 1;
                if (balls[z].PositionMm.X > 1150) GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(1500, 0, 1000));
            }
        }
        public void _Step156(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X > 1250)
                step_11 = 1;
            GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(-1100, 0, 1000));
            
        }
        public void _Step11(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
            {
                if (fish.PolygonVertices[0].X > 1100)
                    step_11 = 56;
                GotoPoint(fish_no, new xna.Vector3(1300, 0, 1000), new xna.Vector3(1300, 0, 1000));
                
                    
            }
            else {

                GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3((balls[z].PositionMm.X - 500), 0, (balls[z].PositionMm.Z + 500)));
                if (balls[z].PositionMm.Z > 900)
                {
                    step_11 = 2;
                }
            
            }
            
        }
        public void _Step21(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_11 = 1;
            GotoPoint(fish_no , new xna.Vector3(1500, 0, 800) , new xna.Vector3(1500, 0, 800));
            if(fish.PolygonVertices[0].X > 1460) {
                step_11 = 3;
            }
        }
        public void _Step31(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_11 = 1;
            GotoPoint(fish_no , new xna.Vector3(2000, 0, 1500) , new xna.Vector3(2000, 0, 1500));
            if (fish.PolygonVertices[0].Z > 930)
            {
                step_11 = 4;
            }
        }
        public void _Step41(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_11 = 1;
            GotoPoint(fish_no , new xna.Vector3(500, 0, 2000) , new xna.Vector3(500, 0, 2000));
            if (fish.PolygonVertices[0].X >= 1250)
                decisions[fish_no].VCode = 4;
            if(fish.PolygonVertices[0].X < 1250)
                decisions[fish_no].VCode = 8;
            
            if (fish.PositionMm.X < (balls[z].PositionMm.X - 20))
                step_11 = 1;
            
            
        }

        public void _Step256(int fish_no, RoboFish fish, int z)
        {
            GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(-1100, 0, -1000));
            if (balls[z].PositionMm.X > 1250)
                step_12 = 1;
        }
        public void _Step12(int fish_no, RoboFish fish, int z)
        {

            if (balls[z].PositionMm.X < 1000)
            {
                if (fish.PolygonVertices[0].X > 1100)
                    step_12 = 56;
                GotoPoint(fish_no, new xna.Vector3(1300, 0, -1000), new xna.Vector3(1300, 0, -1000));
               

            }
            else
            {
                GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3((balls[z].PositionMm.X - 500), 0, (balls[z].PositionMm.Z - 500)));
                if (balls[z].PositionMm.Z < -900)
                {
                    step_12 = 2;
                }
            }

            
        }
        public void _Step22(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_12 = 1;
            GotoPoint(fish_no, new xna.Vector3(1500, 0, -800), new xna.Vector3(1500, 0, -800));
            if (fish.PolygonVertices[0].X > 1460)
            {
                step_12 = 3;
            }
        }
        public void _Step32(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_12 = 1;
            GotoPoint(fish_no, new xna.Vector3(2000, 0, -1500), new xna.Vector3(2000, 0, -1500));
            if (fish.PolygonVertices[0].Z < -930)
            {
                step_12 = 4;
            }
        }
        public void _Step42(int fish_no, RoboFish fish, int z)
        {
            GotoPoint(fish_no, new xna.Vector3(500, 0, -2000), new xna.Vector3(500, 0, -2000));
            if (fish.PolygonVertices[0].X >= 1250)
                decisions[fish_no].VCode = 2;
            if (fish.PolygonVertices[0].X < 1250)
                decisions[fish_no].VCode = 8;
            if (fish.PositionMm.X < (balls[z].PositionMm.X - 20))
                step_12 = 1;
            if (balls[z].PositionMm.X < 1000)
                step_12 = 1;

        }
        public void Step156(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X > 1250)
                step_21 = 1;
            GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(-1100, 0, 1000));
            
        }
        public void Step11(int fish_no, RoboFish fish, int z)
        {

            if (balls[z].PositionMm.X < 1000)
            {
                if (fish.PolygonVertices[0].X > 1100)
                    step_21 = 56;
                GotoPoint(fish_no, new xna.Vector3(1300, 0, 1000), new xna.Vector3(1300, 0, 1000));
                

            }

            else
            {
                GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3((balls[z].PositionMm.X - 500), 0, (balls[z].PositionMm.Z + 500)));
                if (balls[z].PositionMm.Z > 900)
                {
                    step_21 = 2;
                }
 
            }
           
        }
        public void Step21(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_21 = 1;
            GotoPoint(fish_no, new xna.Vector3(1500, 0, 800), new xna.Vector3(1500, 0, 800));
            if (fish.PolygonVertices[0].X > 1460)
            {
                step_21 = 3;
            }
        }
        public void Step31(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_21 = 1;
            GotoPoint(fish_no, new xna.Vector3(2000, 0, 1500), new xna.Vector3(2000, 0, 1500));
            if (fish.PolygonVertices[0].Z > 930)
            {
                step_21 = 4;
            }
        }
        public void Step41(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_21 = 1;
            GotoPoint(fish_no, new xna.Vector3(500, 0, 2000), new xna.Vector3(500, 0, 2000));
            if (fish.PolygonVertices[0].X >= 1250)
                decisions[fish_no].VCode = 4;
            if (fish.PolygonVertices[0].X < 1250)
                decisions[fish_no].VCode = 8;
            if (fish.PositionMm.X < (balls[z].PositionMm.X - 20))
                step_21 = 1;
           

        }

        public void Step256(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X > 1250)
                step_22 = 1;
            GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3(-1100, 0, -1000));
            
        }
        public void Step12(int fish_no, RoboFish fish, int z)
        {

            if (balls[z].PositionMm.X < 1000)
            {
                if (fish.PolygonVertices[0].X > 1100)
                    step_22 = 56;
                GotoPoint(fish_no, new xna.Vector3(1300, 0, -1000), new xna.Vector3(1300, 0, -1000));
                

            }
            else 
            {
                GotoPoint(fish_no, balls[z].PositionMm, new xna.Vector3((balls[z].PositionMm.X - 500), 0, (balls[z].PositionMm.Z - 500)));
                if (balls[z].PositionMm.Z < -900)
                {
                    step_22 = 2;
                }
            }
           
        }
        public void Step22(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_22 = 1;
            GotoPoint(fish_no, new xna.Vector3(1500, 0, -800), new xna.Vector3(1500, 0, -800));
            if (fish.PolygonVertices[0].X > 1460)
            {
                step_22 = 3;
            }
        }
        public void Step32(int fish_no, RoboFish fish, int z)
        {
            if (balls[z].PositionMm.X < 1000)
                step_22 = 1;
            GotoPoint(fish_no, new xna.Vector3(2000, 0, -1500), new xna.Vector3(2000, 0, -1500));
            if (fish.PolygonVertices[0].Z < -930)
            {
                step_22 = 4;
            }
        }
        public void Step42(int fish_no, RoboFish fish, int z)
        {
            GotoPoint(fish_no, new xna.Vector3(500, 0, -2000), new xna.Vector3(500, 0, -2000));
            if (fish.PolygonVertices[0].X >= 1250)
                decisions[fish_no].VCode = 2;
            if (fish.PolygonVertices[0].X < 1250)
                decisions[fish_no].VCode = 8;
            if (fish.PositionMm.X < (balls[z].PositionMm.X - 20))
                step_22 = 1;

            if (balls[z].PositionMm.X < 1000)
                step_22 = 1;

        }
        /// <summary>
        /// 获取当前仿真使命（比赛项目）当前队伍所有仿真机器鱼的决策数据构成的数组
        /// </summary>
        /// <param name="mission">服务端当前运行着的仿真使命Mission对象</param>
        /// <param name="teamId">当前队伍在服务端运行着的仿真使命中所处的编号 
        /// 用于作为索引访问Mission对象的TeamsRef队伍列表中代表当前队伍的元素</param>
        /// <returns>当前队伍所有仿真机器鱼的决策数据构成的Decision数组对象</returns>
        public Decision[] GetDecision(Mission mission, int teamId)
        {

            // 决策类当前对象第一次调用GetDecision时Decision数组引用为null
            if (decisions == null)
            {
                // 根据决策类当前对象对应的仿真使命参与队伍仿真机器鱼的数量分配决策数组空间
                decisions = new Decision[mission.CommonPara.FishCntPerTeam];
                // this.balltemp = mission.EnvRef.Balls;
                /* List<Ball> ball = mission.EnvRef.Balls;
                 for (int i = 0; i <= 8; i++)
                 {
                     //balltemp[i] = new xna.Vector3(ball[i].PositionMm.X,0,ball[i].PositionMm.Z);
                 }*/
            }


            //     int time = 0;  //  用于 平台自带的posetopose参数
            init(mission, teamId); //初始化参数

            //FileStream aFile = new FileStream("C://Users//Herez//Desktop//test.txt", FileMode.OpenOrCreate);
            //StreamWriter sw = new StreamWriter(aFile);
            //aFile.Seek(0, SeekOrigin.End);


            //开始策略

            //策略执行部分
            RoboFish fish0 = mission.TeamsRef[teamId].Fishes[0];
            RoboFish fish1 = mission.TeamsRef[teamId].Fishes[1];
            balls = mission.EnvRef.Balls;
            T = 0;

            //FileStream aFile = new FileStream("C://Users//Herez//Desktop//test.txt", FileMode.OpenOrCreate);//建立一个fileStream对象
            //StreamWriter sw = new StreamWriter(aFile);//用FileStream对像实例一个StreamWriter对象
            //aFile.Seek(0, SeekOrigin.End);
            //sw.WriteLine(step_22);

            //sw.Close();


            /* for (int i = 0; i <= 8; i++)
             {
                 FileStream aFile = new FileStream("G://Balldata.txt", FileMode.OpenOrCreate);//建立一个fileStream对象
                 StreamWriter sw = new StreamWriter(aFile);//用FileStream对像实例一个StreamWriter对象
                 aFile.Seek(0, SeekOrigin.End);
                 sw.WriteLine("_________________________________");
                 sw.WriteLine("球的原坐标------->" + balls[i]);
                 sw.WriteLine("球修正后坐标----->" + balltemp[i]);

                 sw.Close();
             }*/

            //print(time+" ");

            // print("fish2-------->"+maxid);
         // print("fis2----->" + fishstep2);
            //   print(balls[4].VelocityMmPs + " ");
            // close();
            //   print("close-------->" + closeid);
            // print("fish1---->" + fishstep);
            // print("minid---->" + minid + " (X,Z)------>" + balls[minid].PositionMm.X + " " + balls[minid].PositionMm.Z);
            // print("fis2----->" + fish1.PositionMm.Z + "    ----->" + fishstep2);
            // print("maxid----->" + maxid + " (X,Z)------>" + balls[maxid].PositionMm.X + " " + balls[maxid].PositionMm.Z);
            enscore = mission.TeamsRef[(teamId + 1) % 2].Para.Score;
            //  print(" " + enscore);
            //  print("fish---------->" + fishstep2);
            Judge();

            if (enemy() >= 0)
                enemyid = enemy();
            
            //实验到定点函数
            //GotoPoint(0, balls[z].PositionMm, new xna.Vector3((balls[z].PositionMm.X - 500), 0, (balls[z].PositionMm.Z - 750)));


            //上侧扫球（最慢）
            /*
            switch (sel)
            {
                case 1:
                    {
                        GotoPoint(0, balls[z].PositionMm, new xna.Vector3((balls[z].PositionMm.X - 500), 0, (balls[z].PositionMm.Z - 500)));
                        if (fish0.PolygonVertices[0].X > 1480 && fish0.PolygonVertices[0].Z < -950)
                        {
                            sel = 2;
                        }
                    }
                    break;
                case 2:
                    GotoPoint(0, fish0.PolygonVertices[0], new xna.Vector3(fish0.PolygonVertices[0].X + 250, 0, fish0.PolygonVertices[0].Z + 600)); break;
            }
             * /


            //上侧扫球稳定版本
            /*
            switch(sel)
            {
                case 1:
                    {
                        GotoPoint(0, balls[z].PositionMm, new xna.Vector3((balls[z].PositionMm.X - 200), 0, (balls[z].PositionMm.Z - 200)));
                        if(fish0.PolygonVertices[0].X>1450&&fish0.PolygonVertices[0].Z<-950)
                        {
                            sel = 2;
                        }
                    }
                    break;
                case 2:
                    GotoPoint(0, new xna.Vector3(fish0.PolygonVertices[0].X - 50, 0, fish0.PolygonVertices[0].Z - 125), new xna.Vector3(fish0.PolygonVertices[0].X - 100, 0, fish0.PolygonVertices[0].Z - 250)); break;
             
            }
             */
            decisions[0].TCode = 0;
            decisions[0].VCode = 7;
            decisions[1].VCode = 0;
            decisions[1].TCode = 14;

            //switch (sel)
            //{
            //    case 1:
            //        {
            //            GotoPoint(0, new xna.Vector3(balls[z].PositionMm.X + 58, 0, balls[z].PositionMm.Z - 58), new xna.Vector3((balls[z].PositionMm.X - 300), 0, (balls[z].PositionMm.Z - 300)));
            //            if (fish0.PolygonVertices[0].X > 1450 && fish0.PolygonVertices[0].Z < -950)
            //            {
            //                sel = 2;                                        
            //            }
            //        }
            //        break;
            //    case 2:
            //        {
            //            GotoPoint(0, new xna.Vector3(fish0.PolygonVertices[0].X - 50, 0, fish0.PolygonVertices[0].Z - 125), new xna.Vector3(fish0.PolygonVertices[0].X - 100, 0, fish0.PolygonVertices[0].Z - 250));
            //            if (fish0.PolygonVertices[4].X < -1148)
            //            {
            //                sel = 3;
            //                z = 3;
            //            }
            //        }
            //        break;
            //    case 3:
            //        {
            //            GotoPoint(0, new xna.Vector3(balls[z].PositionMm.X-58,0,balls[z].PositionMm.Z-58), new xna.Vector3((balls[z].PositionMm.X + 200), 0, (balls[z].PositionMm.Z + 200)));
            //            if (fish0.PolygonVertices[0].X < -1450 && fish0.PolygonVertices[0].Z < -950)
            //            {
            //                sel = 4;
            //            }

            //        }
            //        break;
            //    case 4:
            //        {
            //            GotoPoint(0, new xna.Vector3(fish0.PolygonVertices[0].X - 100, 0, fish0.PolygonVertices[0].Z + 40), new xna.Vector3(fish0.PolygonVertices[0].X - 250, 0, fish0.PolygonVertices[0].Z + 100));
            //            if(fish0.PolygonVertices[0].Z >-550)
            //            {
            //                sel = 5;
            //            }
            //        }
            //        break;
            //    case 5:
            //        {
            //            GotoPoint(0,fish0.PolygonVertices[0], new xna.Vector3(fish0.PolygonVertices[0].X - 1, 0, fish0.PolygonVertices[0].Z - 1));
        
            //        }
            //        break;

            //}
         //   int a=3;

          //  GotoPoint(0, balls[a].PositionMm, new xna.Vector3((balls[a].PositionMm.X + 500), 0, (balls[a].PositionMm.Z + 500)));


            //
            //上半场左 z1=6 z2=8
            //
            //xna.Vector3 position = mission.EnvRef.Balls[z1].PositionMm;
            //xna.Vector3 position1 = new xna.Vector3(-1100, 0, 800);
            //if (position.X > 1000 && Math.Abs(position.Z) < 500)
            //{
            //    step_11 = 1;
            //    step_12 = 1;
            //    switch (step_1)
            //    {
            //        case 1: _Step1(0, fish0);
            //            break;
            //        case 2: _Step2(0, fish0, z1);
            //            break;
            //        case 3: _Step3(0, fish0, z1);
            //            break;

            //    }


            //}
            //else if (position.X >= 800 && position.Z >= 500)
            //{
            //    step_12 = 1;
            //    step_1 = 1;
            //    switch (step_11)
            //    {
            //        case 1:
            //            _Step11(0, fish0, z1);
            //            break;
            //        case 2:
            //            _Step21(0, fish0, z1);
            //            break;
            //        case 3:
            //            _Step31(0, fish0, z1);
            //            break;
            //        case 4:
            //            _Step41(0, fish0, z1);
            //            break;
            //        case 56:
            //            _Step156(0, fish0, z1);
            //            break;
            //    }

            //}
            //else if (position.X >= 800 && position.Z <= -500)
            //{
            //    step_11 = 1;
            //    step_1 = 1;
            //    switch (step_12)
            //    {
            //        case 1:
            //            _Step12(0, fish0, z1);
            //            break;
            //        case 2:
            //            _Step22(0, fish0, z1);
            //            break;
            //        case 3:
            //            _Step32(0, fish0, z1);
            //            break;
            //        case 4:
            //            _Step42(0, fish0, z1);
            //            break;
            //        case 56:
            //            _Step256(0, fish0, z1);
            //            break;
            //    }
            //}
            //else
            //{
            //    step_11 = 1;
            //    step_12 = 1;
            //    step_1 = 1;
            //    if (position.X < -1050)
            //    {
            //        z1 = first_ballId();
            //        if (z2 == z1) z1 = second_ballId();
            //    }
            //    else GotoPoint(0, position, position1);
            //}

            //鱼2
            //xna.Vector3 position2 = mission.EnvRef.Balls[z2].PositionMm;
            //if (position2.X > 1000 && Math.Abs(position2.Z) < 500)
            //{
            //    step_21 = 1;
            //    step_22 = 1;
            //    switch (step_2)
            //    {
            //        case 1: Step1(1, fish1);
            //            break;
            //        case 2: Step2(1, fish1, z2);
            //            break;
            //        case 3: Step3(1, fish1, z2);
            //            break;

            //    }

            //}
            //else if (position2.X >= 800 && position2.Z >= 500)
            //{
            //    step_22 = 1;
            //    step_2 = 1;
            //    switch (step_21)
            //    {
            //        case 1:
            //            Step11(1, fish1, z2);
            //            break;
            //        case 2:
            //            Step21(1, fish1, z2);
            //            break;
            //        case 3:
            //            Step31(1, fish1, z2);
            //            break;
            //        case 4:
            //            Step41(1, fish1, z2);
            //            break;
            //        case 56:
            //            Step156(1, fish1, z2);
            //            break;
            //    }

            //}

            //else if (position2.X >= 800 && position2.Z <= -500)
            //{
            //    step_21 = 1;
            //    step_2 = 1;
            //    switch (step_22)
            //    {
            //        case 1:
            //            Step12(1, fish1, z2);
            //            break;
            //        case 2:
            //            Step22(1, fish1, z2);
            //            break;
            //        case 3:
            //            Step32(1, fish1, z2);
            //            break;
            //        case 4:
            //            Step42(1, fish1, z2);
            //            break;
            //        case 56:
            //            Step256(1, fish1, z2);
            //            break;
            //    }
            //}
            //else
            //{
            //    step_21 = 1;
            //    step_22 = 1;
            //    step_2 = 1;
            //    if (position2.X < -1050)
            //    {
            //        z2 = first_ballId();
            //        if (z2 == z1) z2 = second_ballId();
            //    }
            //    else GotoPoint(1, position2, position1);
            //}
            



            //print("id---------->" + enemyid);
            /*switch (fishstep)
            {
                case 0: FoneStep0(fish0);
                    break;
                case 1: FoneStep1(fish0, balls);
                    break;
                case 2: FoneStep2(fish0, minid, 1472, -248);
                    break;
                case 3: FoneStep3(fish0, minid);
                    break;
                case 4: FoneStep4(fish0, minid);
                    break;
                case 5: FoneStep5(0, fish0, minid);
                    break;
                case 6: FoneStep6(0, fish0, minid);
                    break;
                case 7: FoneStep7(0, fish0, minid);
                    break;
                case 8: FoneStep8(0, fish0, minid);
                    break;
                case 9: FoneStep9(0, fish0, minid);
                    break;
                case 10: FoneStep10(0, fish0, minid);
                    break;
                case 11: FoneStep11(0, fish0, minid);
                    break;
                case 12: FoneStep12(0, fish0, minid);
                    break;
                case 13: FoneStep13(0, fish0, minid);
                    break;
                /*********************以下开始上半部分，由于平台的原因单独分开来函数实现，以便于微调**********************/
                /*case 14: FoneStep14(0, fish0, minid);
                    break;
                case 15: FoneStep15(0, fish0, minid);
                    break;
                case 16: FoneStep16(0, fish0, minid);
                    break;
                case 17: FoneStep17(0, fish0, minid);
                    break;
                case 18: FoneStep18(0, fish0, minid);
                    break;
                case 19: FoneStep19(0, fish0, minid);
                    break;
                case 20: FoneStep20(0, fish0, minid);
                    break;
                case 21: FoneStep21(0, fish0, minid);
                    break;
                case 22: FoneStep22(0, fish0, minid);
                    break;
                case 23: FoneStep23(0, fish0, minid);
                    break;
                case 24: FoneStep24(0, fish0, minid);
                    break;
                case 25: FoneStep25(0, fish0, minid);
                    break;
                case 26: FoneStep26(0, fish0, minid);
                    break;
                case 27: FoneStep27(0, fish0, minid);
                    break;
                case 28: FoneStep28(0, fish0, minid);
                    break;
                case 29: FoneStep29(0, fish0, minid);
                    break;
                case 30: FoneStep30(0, fish0, minid);
                    break;
                case 31: FoneStep31(0, fish0, minid);
                    break;
            }
            switch (fishstep2)
            {
                case 0: FtwoStep0(1, fish1);
                    break;
                case 1: FtwoStep1(1, fish1);
                    break;
                case 2: GotoNext(fish1);
                    break;
                case 3: GotoNext2(fish1);
                    break;
                case 4: FtwoStep4(1, fish1, maxid);
                    break;
                case 5: FtwoStep5(1, fish1, maxid);
                    break;
                case 6: FtwoStep6(1, fish1, maxid);
                    break;
                case 7: FtwoStep7(1, fish1, maxid);
                    break;
                case 8: FtwoStep8(1, fish1, maxid);
                    break;
                case 9: FtwoStep9(1, fish1, maxid);
                    break;
                case 10: FtwoStep10(1, fish1, maxid);
                    break;
                case 18: FtwoStep18(1, fish1);
                    break;
                case 19: FtwoStep19(1, fish1);
                    break;
                case 20: FtwoStep20(1, fish1);
                    break;
                case 21: FtwoStep21(1, fish1);
                    break;
                case 22: FtwoStep22(1, fish1);
                    break;
                case 23: FtwoStep23(1, fish1);
                    break;
                case 24: FtwoStep24(1, fish1);
                    break;
                case 25: fish2();
                    break;
            }
            //             if (enscore > 0)
            //                fishstep2 = 0;

            /*  FileStream aFile = new FileStream("G://data.txt", FileMode.OpenOrCreate);//建立一个fileStream对象
              StreamWriter sw = new StreamWriter(aFile);//用FileStream对像实例一个StreamWriter对象
              aFile.Seek(0, SeekOrigin.End);
              sw.WriteLine(fishstep);
         

              sw.Close();
          */
            /* getallBall_distance();
             ballin();
             int minid = mindistance_id();
              
             int maxid = maxdistance_id();
             FileStream aFile = new FileStream("e://data.txt", FileMode.OpenOrCreate);//建立一个fileStream对象
             StreamWriter sw = new StreamWriter(aFile);//用FileStream对像实例一个StreamWriter对象
             aFile.Seek(0, SeekOrigin.End);
             sw.WriteLine("min");
             sw.WriteLine(minid);
             sw.WriteLine("max");
             sw.WriteLine(maxid);
               
             sw.Close();
         */
            return decisions;
        }


    }
}