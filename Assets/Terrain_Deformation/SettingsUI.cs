using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public InputField Bekker_Kphi_Input;
    public Slider Bekker_Kphi_Slider;
    public InputField Bekker_Kc_Input;
    public Slider Bekker_Kc_Slider;
    public InputField Bekker_n_Input;
    public Slider Bekker_n_Slider;
    public InputField Mohr_cohesion_Input;
    public Slider Mohr_cohesion_Slider;
    public InputField Mohr_mu_Input;
    public Slider Mohr_mu_Slider;
    public InputField Janosi_shear_Input;
    public Slider Janosi_shear_Slider;
    public InputField elastic_K_Input;
    public Slider elastic_K_Slider;
    public InputField damping_R_Input;
    public Slider damping_R_Slider;
    public InputField granDensity_Input;
    public Slider granDensity_Slider;
    public InputField flowFactor_Input;
    public Slider flowFactor_Slider;
    public Text feedbackText; // текстовое поле для отображения сообщений об ошибках

    void Start()
    {
        SetupSlider(Bekker_Kphi_Slider, Bekker_Kphi_Input, 100, 10000000);
        SetupSlider(Bekker_Kc_Slider, Bekker_Kc_Input, 100, 1000000);
        SetupSlider(Bekker_n_Slider, Bekker_n_Input, 0.1f, 1.6f);
        SetupSlider(Mohr_cohesion_Slider, Mohr_cohesion_Input, 0, 1000);
        SetupSlider(Mohr_mu_Slider, Mohr_mu_Input, 0, 45);
        SetupSlider(Janosi_shear_Slider, Janosi_shear_Input, 0.01f, 1f);
        SetupSlider(elastic_K_Slider, elastic_K_Input, 100, 100000000);
        SetupSlider(damping_R_Slider, damping_R_Input, 0, 1000);
        SetupSlider(granDensity_Slider, granDensity_Input, 100, 10000);
        SetupSlider(flowFactor_Slider, flowFactor_Input, 0.8f, 10f);
    }

    void SetupSlider(Slider slider, InputField inputField, float minValue, float maxValue)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.onValueChanged.AddListener((value) => inputField.text = value.ToString());
        inputField.onEndEdit.AddListener((value) =>
        {
            if (float.TryParse(value, out float result))
            {
                if (result < minValue) result = minValue;
                if (result > maxValue) result = maxValue;
                slider.value = result;
                inputField.text = result.ToString();
            }
        });
    }
    private void FixedUpdate()
    {
       
    }
    public void ApplySettings()
    {
        bool isValid = true;
        feedbackText.text = ""; // Очистка сообщения об ошибках

        double Bekker_Kphi = ValidateInput(Bekker_Kphi_Input.text, 100, 10000000, "Bekker Kphi", ref isValid);
        double Bekker_Kc = ValidateInput(Bekker_Kc_Input.text, 100, 1000000, "Bekker Kc", ref isValid);
        double Bekker_n = ValidateInput(Bekker_n_Input.text, 0.1, 1.6, "Bekker n", ref isValid);
        double Mohr_cohesion = ValidateInput(Mohr_cohesion_Input.text, 0, 1000, "Mohr cohesion", ref isValid);
        double Mohr_mu = ValidateInput(Mohr_mu_Input.text, 0, 45, "Mohr mu", ref isValid);
        double Janosi_shear = ValidateInput(Janosi_shear_Input.text, 0.01, 1, "Janosi shear", ref isValid);
        double elastic_K = ValidateInput(elastic_K_Input.text, 100, 100000000, "Elastic K", ref isValid);
        double damping_R = ValidateInput(damping_R_Input.text, 0, 1000, "Damping R", ref isValid);
        double granDensity = ValidateInput(granDensity_Input.text, 100, 10000, "Gran density", ref isValid);
        double flowFactor = ValidateInput(flowFactor_Input.text, 0.8, 1.2, "Flow factor", ref isValid);

        if (isValid)
        {
            PhysicalFootprint.Bekker_Kphi = Bekker_Kphi;
            PhysicalFootprint.Bekker_Kc = Bekker_Kc;
            PhysicalFootprint.Bekker_n = Bekker_n;
            PhysicalFootprint.Mohr_cohesion = Mohr_cohesion;
            PhysicalFootprint.Mohr_mu = Mohr_mu;
            PhysicalFootprint.Janosi_shear = Janosi_shear;
            PhysicalFootprint.elastic_K = elastic_K;
            PhysicalFootprint.damping_R = damping_R;
            PhysicalFootprint.granDensity = granDensity;
            PhysicalFootprint.flowFactor = flowFactor;

            feedbackText.text = "Параметры успешно применены!";
        }
        else
        {
            feedbackText.text = "Ошибка применения параметров. Проверьте значения";
        }
    }

    private double ValidateInput(string input, double minValue, double maxValue, string variableName, ref bool isValid)
    {
        if (double.TryParse(input, out double value))
        {
            if (value < minValue || value > maxValue)
            {
                feedbackText.text += $"{variableName} должно быть больше {minValue} и {maxValue}.\n";
                isValid = false;
            }
            return value;
        }
        else
        {
            feedbackText.text += $"{variableName} не действительное число.\n";
            isValid = false;
            return 0; // Возвращаемое значение в случае ошибки
        }
    }
    public void StartSimulation()
    {
        SceneManager.LoadScene(1);
    }
    public void SetSand()
    {  
        double Bekker_Kphi = 15000;
        double Bekker_Kc = 2000;
        double Bekker_n = 1.6;
        double Mohr_cohesion = 100;
        double Mohr_mu = 30;
        double Janosi_shear = 0.02;
        double elastic_K = 250000;
        double damping_R = 50;
        double granDensity = 1000;
        double flowFactor = 1.1;

        Bekker_Kphi_Input.text = Bekker_Kphi.ToString();
        Bekker_Kc_Input.text = Bekker_Kc.ToString();
        Bekker_n_Input.text = Bekker_n.ToString();
        Mohr_cohesion_Input.text = Mohr_cohesion.ToString();
        Mohr_mu_Input.text = Mohr_mu.ToString();
        Janosi_shear_Input.text = Janosi_shear.ToString();
        elastic_K_Input.text = elastic_K.ToString();
        damping_R_Input.text = damping_R.ToString();
        granDensity_Input.text = granDensity.ToString();
        flowFactor_Input.text = flowFactor.ToString();

        PhysicalFootprint.Bekker_Kphi = Bekker_Kphi;
        PhysicalFootprint.Bekker_Kc = Bekker_Kc;
        PhysicalFootprint.Bekker_n = Bekker_n;
        PhysicalFootprint.Mohr_cohesion = Mohr_cohesion;
        PhysicalFootprint.Mohr_mu = Mohr_mu;
        PhysicalFootprint.Janosi_shear = Janosi_shear;
        PhysicalFootprint.elastic_K = elastic_K;
        PhysicalFootprint.damping_R = damping_R;
        PhysicalFootprint.granDensity = granDensity;
        PhysicalFootprint.flowFactor = flowFactor;
       

    }
    public void SetMud()
    {
        double Bekker_Kphi = 300000;
        double Bekker_Kc = 50000;
        double Bekker_n = 1.4;
        double Mohr_cohesion = 350;
        double Mohr_mu = 40;
        double Janosi_shear = 0.1;
        double elastic_K = 600000;
        double damping_R = 200;
        double granDensity = 800;
        double flowFactor = 1.2;

        Bekker_Kphi_Input.text = Bekker_Kphi.ToString();
        Bekker_Kc_Input.text = Bekker_Kc.ToString();
        Bekker_n_Input.text = Bekker_n.ToString();
        Mohr_cohesion_Input.text = Mohr_cohesion.ToString();
        Mohr_mu_Input.text = Mohr_mu.ToString();
        Janosi_shear_Input.text = Janosi_shear.ToString();
        elastic_K_Input.text = elastic_K.ToString();
        damping_R_Input.text = damping_R.ToString();
        granDensity_Input.text = granDensity.ToString();
        flowFactor_Input.text = flowFactor.ToString();

        PhysicalFootprint.Bekker_Kphi = Bekker_Kphi;
        PhysicalFootprint.Bekker_Kc = Bekker_Kc;
        PhysicalFootprint.Bekker_n = Bekker_n;
        PhysicalFootprint.Mohr_cohesion = Mohr_cohesion;
        PhysicalFootprint.Mohr_mu = Mohr_mu;
        PhysicalFootprint.Janosi_shear = Janosi_shear;
        PhysicalFootprint.elastic_K = elastic_K;
        PhysicalFootprint.damping_R = damping_R;
        PhysicalFootprint.granDensity = granDensity;
        PhysicalFootprint.flowFactor = flowFactor;
        PhysicalFootprint.useCohesion = true;

    }
    public void SetGravel()
    {
        double Bekker_Kphi = 400000;
        double Bekker_Kc = 20000;
        double Bekker_n = 1.1;
        double Mohr_cohesion = 20;
        double Mohr_mu = 35;
        double Janosi_shear = 0.5;
        double elastic_K = 1000000;
        double damping_R = 100;
        double granDensity = 1600;
        double flowFactor = 2.4;

        Bekker_Kphi_Input.text = Bekker_Kphi.ToString();
        Bekker_Kc_Input.text = Bekker_Kc.ToString();
        Bekker_n_Input.text = Bekker_n.ToString();
        Mohr_cohesion_Input.text = Mohr_cohesion.ToString();
        Mohr_mu_Input.text = Mohr_mu.ToString();
        Janosi_shear_Input.text = Janosi_shear.ToString();
        elastic_K_Input.text = elastic_K.ToString();
        damping_R_Input.text = damping_R.ToString();
        granDensity_Input.text = granDensity.ToString();
        flowFactor_Input.text = flowFactor.ToString();

        PhysicalFootprint.Bekker_Kphi = Bekker_Kphi;
        PhysicalFootprint.Bekker_Kc = Bekker_Kc;
        PhysicalFootprint.Bekker_n = Bekker_n;
        PhysicalFootprint.Mohr_cohesion = Mohr_cohesion;
        PhysicalFootprint.Mohr_mu = Mohr_mu;
        PhysicalFootprint.Janosi_shear = Janosi_shear;
        PhysicalFootprint.elastic_K = elastic_K;
        PhysicalFootprint.damping_R = damping_R;
        PhysicalFootprint.granDensity = granDensity;
        PhysicalFootprint.flowFactor = flowFactor;

    }
    public void SetTorph()
    {
        double Bekker_Kphi = 50000;
        double Bekker_Kc = 5000;
        double Bekker_n = 1.6;
        double Mohr_cohesion = 50;
        double Mohr_mu = 10;
        double Janosi_shear = 0.01;
        double elastic_K = 200000;
        double damping_R = 20;
        double granDensity = 9000;
        double flowFactor = 1.2;

        Bekker_Kphi_Input.text = Bekker_Kphi.ToString();
        Bekker_Kc_Input.text = Bekker_Kc.ToString();
        Bekker_n_Input.text = Bekker_n.ToString();
        Mohr_cohesion_Input.text = Mohr_cohesion.ToString();
        Mohr_mu_Input.text = Mohr_mu.ToString();
        Janosi_shear_Input.text = Janosi_shear.ToString();
        elastic_K_Input.text = elastic_K.ToString();
        damping_R_Input.text = damping_R.ToString();
        granDensity_Input.text = granDensity.ToString();
        flowFactor_Input.text = flowFactor.ToString();

        PhysicalFootprint.Bekker_Kphi = Bekker_Kphi;
        PhysicalFootprint.Bekker_Kc = Bekker_Kc;
        PhysicalFootprint.Bekker_n = Bekker_n;
        PhysicalFootprint.Mohr_cohesion = Mohr_cohesion;
        PhysicalFootprint.Mohr_mu = Mohr_mu;
        PhysicalFootprint.Janosi_shear = Janosi_shear;
        PhysicalFootprint.elastic_K = elastic_K;
        PhysicalFootprint.damping_R = damping_R;
        PhysicalFootprint.granDensity = granDensity;
        PhysicalFootprint.flowFactor = flowFactor;
        PhysicalFootprint.useCohesion = true;

    }
    public void SetSupes()
    {
        double Bekker_Kphi = 250000;
        double Bekker_Kc = 40000;
        double Bekker_n = 1.2;
        double Mohr_cohesion = 30;
        double Mohr_mu = 25;
        double Janosi_shear = 0.03;
        double elastic_K = 500000;
        double damping_R = 80;
        double granDensity = 1000;
        double flowFactor = 0.9;

        Bekker_Kphi_Input.text = Bekker_Kphi.ToString();
        Bekker_Kc_Input.text = Bekker_Kc.ToString();
        Bekker_n_Input.text = Bekker_n.ToString();
        Mohr_cohesion_Input.text = Mohr_cohesion.ToString();
        Mohr_mu_Input.text = Mohr_mu.ToString();
        Janosi_shear_Input.text = Janosi_shear.ToString();
        elastic_K_Input.text = elastic_K.ToString();
        damping_R_Input.text = damping_R.ToString();
        granDensity_Input.text = granDensity.ToString();
        flowFactor_Input.text = flowFactor.ToString();

        PhysicalFootprint.Bekker_Kphi = Bekker_Kphi;
        PhysicalFootprint.Bekker_Kc = Bekker_Kc;
        PhysicalFootprint.Bekker_n = Bekker_n;
        PhysicalFootprint.Mohr_cohesion = Mohr_cohesion;
        PhysicalFootprint.Mohr_mu = Mohr_mu;
        PhysicalFootprint.Janosi_shear = Janosi_shear;
        PhysicalFootprint.elastic_K = elastic_K;
        PhysicalFootprint.damping_R = damping_R;
        PhysicalFootprint.granDensity = granDensity;
        PhysicalFootprint.flowFactor = flowFactor;
        PhysicalFootprint.useCohesion = true;

    }
    public void SadSoil()
    {
        double Bekker_Kphi = 300000;
        double Bekker_Kc = 30000;
        double Bekker_n = 1.2;
        double Mohr_cohesion = 300;
        double Mohr_mu = 30;
        double Janosi_shear = 0.05;
        double elastic_K = 1000000;
        double damping_R = 100;
        double granDensity = 1100;
        double flowFactor = 0.9;

        Bekker_Kphi_Input.text = Bekker_Kphi.ToString();
        Bekker_Kc_Input.text = Bekker_Kc.ToString();
        Bekker_n_Input.text = Bekker_n.ToString();
        Mohr_cohesion_Input.text = Mohr_cohesion.ToString();
        Mohr_mu_Input.text = Mohr_mu.ToString();
        Janosi_shear_Input.text = Janosi_shear.ToString();
        elastic_K_Input.text = elastic_K.ToString();
        damping_R_Input.text = damping_R.ToString();
        granDensity_Input.text = granDensity.ToString();
        flowFactor_Input.text = flowFactor.ToString();

        PhysicalFootprint.Bekker_Kphi = Bekker_Kphi;
        PhysicalFootprint.Bekker_Kc = Bekker_Kc;
        PhysicalFootprint.Bekker_n = Bekker_n;
        PhysicalFootprint.Mohr_cohesion = Mohr_cohesion;
        PhysicalFootprint.Mohr_mu = Mohr_mu;
        PhysicalFootprint.Janosi_shear = Janosi_shear;
        PhysicalFootprint.elastic_K = elastic_K;
        PhysicalFootprint.damping_R = damping_R;
        PhysicalFootprint.granDensity = granDensity;
        PhysicalFootprint.flowFactor = flowFactor;
        PhysicalFootprint.useCohesion = true;


    }
    public void HardSoil()
    {
        double Bekker_Kphi = 2e6;
        double Bekker_Kc = 4e4;
        double Bekker_n = 0.9;
        double Mohr_cohesion = 300;
        double Mohr_mu = 30;
        double Janosi_shear = 0.05;
        double elastic_K = 5e7;
        double damping_R = 100;
        double granDensity = 3400;
        double flowFactor = 0.9;

        Bekker_Kphi_Input.text = Bekker_Kphi.ToString();
        Bekker_Kc_Input.text = Bekker_Kc.ToString();
        Bekker_n_Input.text = Bekker_n.ToString();
        Mohr_cohesion_Input.text = Mohr_cohesion.ToString();
        Mohr_mu_Input.text = Mohr_mu.ToString();
        Janosi_shear_Input.text = Janosi_shear.ToString();
        elastic_K_Input.text = elastic_K.ToString();
        damping_R_Input.text = damping_R.ToString();
        granDensity_Input.text = granDensity.ToString();
        flowFactor_Input.text = flowFactor.ToString();

        PhysicalFootprint.Bekker_Kphi = Bekker_Kphi;
        PhysicalFootprint.Bekker_Kc = Bekker_Kc;
        PhysicalFootprint.Bekker_n = Bekker_n;
        PhysicalFootprint.Mohr_cohesion = Mohr_cohesion;
        PhysicalFootprint.Mohr_mu = Mohr_mu;
        PhysicalFootprint.Janosi_shear = Janosi_shear;
        PhysicalFootprint.elastic_K = elastic_K;
        PhysicalFootprint.damping_R = damping_R;
        PhysicalFootprint.granDensity = granDensity;
        PhysicalFootprint.flowFactor = flowFactor;
        PhysicalFootprint.useCohesion = true;


    }

}
