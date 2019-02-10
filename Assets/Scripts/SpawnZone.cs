﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnZone : PersistableObject
{
    public abstract Vector3 SpawnPoint { get; }
    public virtual void SpawnShape() {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();

        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        SetupColor(shape);

        float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
        if (angularSpeed != 0f) {
            var rotation = shape.AddBehavior<RotationShapeBehavior>();
            rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
        }

        float speed = spawnConfig.speed.RandomValueInRange;
        if (speed != 0f) {
            var movement = shape.AddBehavior<MovementShapeBehavior>();
            movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
        }
        SetupOscillation(shape);
        CreateSatelliteFor(shape);
    }

    Vector3 GetDirectionVector(SpawnConfiguration.MovementDirection direction, Transform t) {
        switch (spawnConfig.movementDirection) {
            case SpawnConfiguration.MovementDirection.Upward:
                return transform.up;
            case SpawnConfiguration.MovementDirection.Outward:
                return (t.localPosition - transform.position).normalized;
            case SpawnConfiguration.MovementDirection.Random:
                return Random.onUnitSphere;
            default:
                return transform.forward;
        }
    }

    void SetupOscillation(Shape shape) {
        float amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
        float frequency = spawnConfig.oscillationFrequency.RandomValueInRange;
        if (amplitude == 0f || frequency == 0f) {
            return;
        }
        var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
        oscillation.Offset = GetDirectionVector(spawnConfig.oscillationDirection, shape.transform) * amplitude;
        oscillation.Frequency = frequency;
    }


    void CreateSatelliteFor(Shape focalShape) {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();

        Transform t = shape.transform;
        t.localRotation = Random.rotation;
        t.localScale = focalShape.transform.localScale * 0.5f;
        t.localPosition = focalShape.transform.localPosition + Vector3.up;
        shape.AddBehavior<MovementShapeBehavior>().Velocity = Vector3.up;
        SetupColor(shape);
        shape.AddBehavior<SatelliteShapeBehavior>().Initialize(
            shape, focalShape, 
            spawnConfig.satellite.orbitRadius.RandomValueInRange, 
            spawnConfig.satellite.orbitFrequency.RandomValueInRange
        );
        int satelliteCount = spawnConfig.satellite.amount.RandomValueInRange;
        for (int i = 0; i < satelliteCount; i++) {
            CreateSatelliteFor(shape);
        }
    }

    void SetupColor(Shape shape) {
        if (spawnConfig.uniformColor) {
            shape.SetColor(spawnConfig.color.RandomInRange);
        } else {
            for (int i = 0; i < shape.ColorCount; i++) {
                shape.SetColor(spawnConfig.color.RandomInRange, i);
            }
        }
    }

    [System.Serializable]
    public struct SpawnConfiguration {
        public enum MovementDirection {
            Forward, Upward, Outward, Random
        }
        public ShapeFactory[] factories;
        public MovementDirection movementDirection;
        public FloatRange speed;
        public FloatRange angularSpeed;
        public FloatRange scale;
        public ColorRangeHSV color;
        public bool uniformColor;
        // 振荡方向
        public MovementDirection oscillationDirection;
        // 振荡幅度
        public FloatRange oscillationAmplitude;
        // 振荡频率
        public FloatRange oscillationFrequency;
        [System.Serializable]
        public struct SatelliteConfiguration {
            public IntRange amount;
            [FloatRangeSlider(0.1f, 1f)]
            public FloatRange relativeScale;
            public FloatRange orbitRadius;
            public FloatRange orbitFrequency;
        }

        public SatelliteConfiguration satellite;
    }

    [SerializeField]
    SpawnConfiguration spawnConfig;



}
