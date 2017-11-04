#pragma once

#include <stdio.h>
#include <math.h>
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include "cuda.h"
#include <time.h>
#include "Types.h"

#define MAX_OBS_COUNT 19
#define ROBOT_FORCE 0.3
#define BALL_FORCE 0.2
#define ZONE_FORCE 1.2		//1
#define OPP_ZONE_FORCE 1.3 //1.1
//texture<float, 2, cudaReadModeElementType> texA;
texture<float, 2, cudaReadModeElementType> texB;
texture<float, 2, cudaReadModeElementType> texC;
texture<float, 2, cudaReadModeElementType> texF;
texture<float, 2, cudaReadModeElementType> texV;

float *DevForce, *DevPath, *tmp;
int MaxPathCount, MaxRRTCount;
size_t ForcePitch, PathPitch, ObsPitch, AvoidPitch;
float* DevObs;
float* DevAvoid;
int* DevEachPathCount;
#define PI  3.14159

float _kSpring = 0.3, _kSpring2 = 0.3;
int N = 2;
size_t pathOffset;

//const int threadsPerBlock = 256;    
unsigned int memory_total;
unsigned int memory_free;
__global__ void CalculateForcesKernel(float* Force, int* EachPathCount, int RobotCount,size_t ForcePitch,float _kSpring,float _kSpring2,int N);
__global__ void ReCalculatePath(float* Path, int* DevEachPathCount,int RobotCount,size_t PathPitch,int ObstacleCount);
void  DisposeElastic();
void  ElasticInit(int, int);