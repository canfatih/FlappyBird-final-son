using UnityEngine;
using System.Collections;

// Fatih Can
// 360118507
public class FlappyScript : MonoBehaviour
{

    public AudioClip FlyAudioClip, DeathAudioClip, ScoredAudioClip; // sesler
    public Sprite GetReadySprite;
    public float RotateUpSpeed = 1, RotateDownSpeed = 1; //aşağı yukarı hız katsayısı
    public GameObject IntroGUI, DeathGUI;
    public Collider2D restartButtonGameCollider;
    public float VelocityPerJump = 3; // 3 birim zıplar
    public float XSpeed = 1;

    // Use this for initialization
    void Start()
    {

    }

    FlappyYAxisTravelState flappyYAxisTravelState;

    // kuş yukarı ya da aşağı gider durumda olabilir sabit durmuyor
    enum FlappyYAxisTravelState
    {
        GoingUp, GoingDown
    }

    Vector3 birdRotation = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        // esc ile oyundan çıkış
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        // başlangıç durumunda
        if (GameStateManager.GameState == GameState.Intro)
        {
            MoveBirdOnXAxis();
            if (WasTouchedOrClicked())
            {
                BoostOnYAxis();
                GameStateManager.GameState = GameState.Playing;
                IntroGUI.SetActive(false);
                ScoreManagerScript.Score = 0;
            }
        }

        // oyun içinde
        else if (GameStateManager.GameState == GameState.Playing)
        {
            MoveBirdOnXAxis();
            if (WasTouchedOrClicked())
            {
                BoostOnYAxis();
            }

        }

        // oyun sonunda
        else if (GameStateManager.GameState == GameState.Dead)
        {
            Vector2 contactPoint = Vector2.zero;

            if (Input.touchCount > 0)
                contactPoint = Input.touches[0].position;
            if (Input.GetMouseButtonDown(0))
                contactPoint = Input.mousePosition;

            // karakter öldü yeniden başlatmayı sor
            if (restartButtonGameCollider == Physics2D.OverlapPoint
                (Camera.main.ScreenToWorldPoint(contactPoint)))
            {
                GameStateManager.GameState = GameState.Intro;
                Application.LoadLevel(Application.loadedLevelName);
            }
        }

    }


    void FixedUpdate()
    {
        // oyun başlangıcında yerinde zıplama
        if (GameStateManager.GameState == GameState.Intro)
        {
            if (GetComponent<Rigidbody2D>().velocity.y < -1) //aşağı düşmeye başladığında
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, GetComponent<Rigidbody2D>().mass * 5500 * Time.deltaTime)); //yukarı ittir
                                                        
        }
        else if (GameStateManager.GameState == GameState.Playing || GameStateManager.GameState == GameState.Dead)
        {
            FixFlappyRotation();
        }
    }

    // tıklamak yukarı tuşu ve dokunmak
    bool WasTouchedOrClicked()
    {
        if (Input.GetButtonUp("Jump") || Input.GetMouseButtonDown(0) || 
            (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
            return true;
        else
            return false;
    }

    // yer ilerler kuş yerinde kalır
    void MoveBirdOnXAxis()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0, 0);
    }

    // ilk zıplama
    void BoostOnYAxis()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, VelocityPerJump);
        GetComponent<AudioSource>().PlayOneShot(FlyAudioClip);
    }



    // karakter yukarı giderken yüzünü 45 dereceye kadar yukarı dönüyor,
    // aşağı giderken ise 90 dereceye kadar yüzünü aşağı dönüyor ve burun üstü düşebiliyor.
    private void FixFlappyRotation()
    {
        // dikey hızı sıfırdan büyükse yukarı küçük ise aşağı gidiyor.
        if (GetComponent<Rigidbody2D>().velocity.y > 0) flappyYAxisTravelState = FlappyYAxisTravelState.GoingUp;
        else flappyYAxisTravelState = FlappyYAxisTravelState.GoingDown;

        float degreesToAdd = 0;

        switch (flappyYAxisTravelState)
        {
            case FlappyYAxisTravelState.GoingUp:
                degreesToAdd = 6 * RotateUpSpeed;
                break;
            case FlappyYAxisTravelState.GoingDown:
                degreesToAdd = -3 * RotateDownSpeed;
                break;
            default:
                break;
        }

        // kuşun yüzü her zaman -90<arasındadır<45 
        birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z + degreesToAdd, -90, 45));
        transform.eulerAngles = birdRotation;
    }

    // Kuşun borular ve boru boşlukları ile etkileşimi
    // puan kazanıyor veya ölüyor.
    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            //pipeblank iki borunun arasındaki boşluğun adı. buraya çarpmak bir aşama geçmek demek
            if (col.gameObject.tag == "Pipeblank") 
            {
                // başarılı geçti sesi ve puan arttırma
                GetComponent<AudioSource>().PlayOneShot(ScoredAudioClip);
                ScoreManagerScript.Score++;
            }
            // boruya çarptı demek
            else if (col.gameObject.tag == "Pipe")
            {
                FlappyDies();
            }
        }
    }

    // Kuş yere çarparsa ölüyor
    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Floor")
            {
                FlappyDies();
            }
        }
    }

    // oyun bitti durumuna geçiyor.
    void FlappyDies()
    {
        GameStateManager.GameState = GameState.Dead;
        DeathGUI.SetActive(true);
        // ölüm müziği
        GetComponent<AudioSource>().PlayOneShot(DeathAudioClip);
    }

}
