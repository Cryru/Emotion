using System;
using System.Collections.Generic;
using System.Text;

namespace Adfectus.ExecTest
{
    public static class MonitorSimulator
    {

        private static Random _rng = new Random();

        private static double monitorRefreshRate = 60;
        private static bool vSync = true;


        public static double GameUpdateTime()
        {
            return .00001;
        }

        public static double GameRenderTime()
        {
            return .005;
        }

        public static double GameDisplayTime()
        {
            return .000001;
        }

        public static double BusyTime()
        {
            return .000001;
        }

        private static double systemTime = 0;

        private static double timingFuzziness = 1.0 / 60.0 * 0.005;
        public static double Fuzzy()
        {
            double duble = _rng.NextDouble();
            int negPos = _rng.Next(0, 2);

            double valche = timingFuzziness * duble;

            if (negPos == 0)
            {
                return timingFuzziness;
            }
            else
            {
                return timingFuzziness * -1;
            }
        }

        //measurements
        public static int frame_updates = 0;
        public static double total_updates = 0;
        public static int last_vsync = 0;
        public static int missed_updates = 0;
        public static int double_updates = 0;

        public static void simulate_update()
        {
            systemTime += Math.Max(0.0, GameUpdateTime() + Fuzzy() * .01);
            total_updates++;
            frame_updates++;
        }
        public static void simulate_render()
        {
            systemTime += Math.Max(0.0, GameRenderTime() + Fuzzy() * .01);
        }

        public static void simulate_display()
        {
            if (vSync)
            {
                systemTime += Math.Max(0.0, (Math.Ceiling(systemTime * monitorRefreshRate) / monitorRefreshRate) - systemTime + Fuzzy());
            }
            else
            {
                systemTime += Math.Max(0.0, GameDisplayTime() + Fuzzy());
            }


            int current_vsync = (int)Math.Round(systemTime * monitorRefreshRate);
            if (last_vsync != current_vsync)
            {
                for (int i = last_vsync; i < current_vsync - 1; i++)
                {
                    Console.Write(0);
                    missed_updates++;
                }
                Console.Write(frame_updates);
                if (frame_updates > 1) double_updates++;
                last_vsync = current_vsync;

                frame_updates = 0;
            }

        }

        public static void simulate_busy()
        {
            systemTime += Math.Max(0.0, BusyTime() + Fuzzy() * .00001);
        }

        //this is where you test your game loop
        public static int main()
        {
            double prev_frame_time = systemTime;
            last_vsync = (int) Math.Round(systemTime * monitorRefreshRate);
            int first_vsync = last_vsync;

            double accumulator = 0;

            while (total_updates < 10000)
            {
                double current_frame_time = systemTime;
                double delta_frame_time = current_frame_time - prev_frame_time;
                accumulator += delta_frame_time;
                prev_frame_time = current_frame_time;

                while (accumulator >= 1.0 / 60.0)
                {
                    simulate_update();
                    accumulator -= 1.0 / 60.0;
                }

                simulate_render();
                simulate_display();
                simulate_busy();
            }

         
           Console.WriteLine($"TOTAL UPDATES: {total_updates}");
           Console.WriteLine($"TOTAL VSYNCS: {last_vsync - first_vsync}");
           Console.WriteLine($"TOTAL DOUBLE UPDATES: {double_updates}");
           Console.WriteLine($"TOTAL SKIPPED RENDERS: {missed_updates}");

           Console.WriteLine($"GAME TIME: {total_updates * (1.0 / 60.0)}");
           Console.WriteLine($"SYSTEM TIME: {(last_vsync - first_vsync) / monitorRefreshRate}");

           return 1;
        }
    }
}

/**

//measurements
int frame_updates = 0;
double total_updates = 0;
int last_vsync = 0;
int missed_updates = 0;
int double_updates = 0;


void simulate_update(){
    system_time += std::max(0.0, game_update_time() + fuzzy() * .01);
    total_updates++;
    frame_updates++;
}
void simulate_render(){
    system_time += std::max(0.0, game_render_time() + fuzzy() * .01);
}
void simulate_display(){
    if(vsync){
        system_time += std::max(0.0, (ceil(system_time * monitor_refresh_rate) / monitor_refresh_rate) - system_time + fuzzy()); 
    } else {
        system_time += std::max(0.0, game_display_time() + fuzzy());
    }


    int current_vsync = round(system_time * monitor_refresh_rate);
    if(last_vsync != current_vsync){
        for(int i = last_vsync; i<current_vsync-1; i++){
           Console.WriteLine(0;
            missed_updates++;
        }
       Console.WriteLine(frame_updates;
        if(frame_updates > 1) double_updates++;
        last_vsync = current_vsync;

        frame_updates = 0;
    }
    
}
void simulate_busy(){
    system_time += std::max(0.0, busy_time() + fuzzy() * .00001);
}

//this is where you test your game loop
int main() {
    double prev_frame_time = system_time;
    last_vsync = round(system_time * monitor_refresh_rate);
    int first_vsync = last_vsync;

    double accumulator = 0;

    while(total_updates < 10000){
        double current_frame_time = system_time;
        double delta_frame_time = current_frame_time - prev_frame_time;
        accumulator += delta_frame_time;
        prev_frame_time = current_frame_time;

        while(accumulator >= 1.0 / 60.0){
            simulate_update();
            accumulator -= 1.0 / 60.0;
        }

        simulate_render();
        simulate_display();
        simulate_busy();
    }

   Console.WriteLine(std::endl);
   Console.WriteLine("TOTAL UPDATES: " << total_updates);
   Console.WriteLine("TOTAL VSYNCS: " << last_vsync-first_vsync);
   Console.WriteLine("TOTAL DOUBLE UPDATES: " << double_updates);
   Console.WriteLine("TOTAL SKIPPED RENDERS: " << missed_updates);

   Console.WriteLine("GAME TIME: " << total_updates*(1.0/60.0));
   Console.WriteLine("SYSTEM TIME: " << (last_vsync-first_vsync)/monitor_refresh_rate);
}
 *
 */
