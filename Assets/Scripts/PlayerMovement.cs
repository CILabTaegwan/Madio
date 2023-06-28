using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;
    private new Collider2D collider;

    private Vector2 velocity;
    private float inputAxis;
    
    private Stopwatch stopwatch;

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool yelling { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);
    public bool falling => velocity.y < 0f && !grounded;

    public string microphoneDevice;
    public float sensitivity = 100f;

    private AudioSource audioSource;
    private void Awake()
    {
        stopwatch = Stopwatch.StartNew();
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        
        
        audioSource = gameObject.AddComponent<AudioSource>();

        // Check for available microphone devices
        if (Microphone.devices.Length > 0)
        {
            // Set the microphone device
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            // Debug.LogError("No microphone device found!");
        }

        // Start recording from the microphone
        audioSource.clip = Microphone.Start(microphoneDevice, true, 1, AudioSettings.outputSampleRate);
        audioSource.loop = true;

        // Check if the microphone is recording
        if (Microphone.IsRecording(microphoneDevice))
        {
            // Play the recorded audio to analyze the dB levels
            audioSource.Play();
        }
        else
        {
            // Debug.LogError("Microphone recording failed!");
        }    
    }

    private void OnEnable()
    {
        rigidbody.isKinematic = false;
        collider.enabled = true;
        velocity = Vector2.zero;
        jumping = false;
        yelling = false;
    }

    private void OnDisable()
    {
        rigidbody.isKinematic = true;
        collider.enabled = false;
        velocity = Vector2.zero;
        jumping = false;
        yelling = false;
    }

    private void Update()
    {
        // Debug.Log("dB Level: " + dbLevel);
        
        HorizontalMovement();

        grounded = rigidbody.Raycast(Vector2.down);

        if (grounded)
        {
            GroundedMovement();
        }

        ApplyGravity();
    }

    private void FixedUpdate()
    {
        // move mario based on his velocity
        Vector2 position = rigidbody.position;
        position += velocity * Time.fixedDeltaTime;

        // clamp within the screen bounds
        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);

        rigidbody.MovePosition(position);
    }

    private void HorizontalMovement()
    {
        // accelerate / decelerate
        inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * moveSpeed, moveSpeed * Time.deltaTime);

        // check if running into a wall
        if (rigidbody.Raycast(Vector2.right * velocity.x)) {
            velocity.x = 0f;
        }

        // flip sprite to face direction
        if (velocity.x > 0f) {
            transform.eulerAngles = Vector3.zero;
        } else if (velocity.x < 0f) {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void GroundedMovement()
    {
        float dbLevel = 0.0f;

        Debug.Log(stopwatch.ElapsedMilliseconds);
        if (stopwatch.ElapsedMilliseconds > 1500)
        {
            dbLevel = GetDBLevel();
            yelling = false;
        }
        // prevent gravity from infinitly building up
        velocity.y = Mathf.Max(velocity.y, 0f);
        jumping = velocity.y > 0f;

        // perform jump
        // if (Input.GetButtonDown("Jump"))
        if (dbLevel > 10.0 && !yelling)
        {
            // Debug.Log(jumpForce);
            // velocity.y = jumpForce;
            velocity.y = dbLevel;
            jumping = true;
            yelling = true;
            stopwatch.Restart();
        }
    }

    private void ApplyGravity()
    {
        // check if falling
        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = falling ? 2f : 1f;

        // apply gravity and terminal velocity
        velocity.y += gravity * multiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // bounce off enemy head
            if (transform.DotTest(collision.transform, Vector2.down))
            {
                velocity.y = jumpForce / 2f;
                jumping = true;
            }
        }
        else if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
        {
            // stop vertical movement if mario bonks his head
            if (transform.DotTest(collision.transform, Vector2.up)) {
                velocity.y = 0f;
            }
        }
    }

    float GetDBLevel()
    {
        // Get a block of audio samples from the microphone
        float[] samples = new float[audioSource.clip.samples];
        audioSource.clip.GetData(samples, 0);

        // Calculate the number of samples to consider based on the capture duration
        int numSamplesToConsider = Mathf.CeilToInt(0.5f * audioSource.clip.frequency);

        // Calculate the starting sample index for the duration
        int startSampleIndex = samples.Length - numSamplesToConsider;

        // Find the highest dB level among the samples within the duration
        float highestDBLevel = float.NegativeInfinity;
        for (int i = startSampleIndex; i < samples.Length; i++)
        {
            float sample = samples[i];

            // Calculate the dB level for the current sample
            float rms = Mathf.Pow(sample, 2);
            float dbLevel = 20f * Mathf.Log10(sensitivity * Mathf.Sqrt(rms));

            // Update the highest dB level if necessary
            if (dbLevel > highestDBLevel)
            {
                highestDBLevel = dbLevel;
            }
        }

        return highestDBLevel;
        
        // Calculate the RMS value of the audio samples
        // float rms = 0f;
        // foreach (float sample in samples)
        // {
        //     rms += Mathf.Pow(sample, 2);
        // }
        // rms /= samples.Length;
        // rms = Mathf.Sqrt(rms);
        //
        // // Convert RMS value to dB using sensitivity value
        // float dbLevel = 20f * Mathf.Log10(sensitivity * rms);
        //
        // return dbLevel;
    }
}
