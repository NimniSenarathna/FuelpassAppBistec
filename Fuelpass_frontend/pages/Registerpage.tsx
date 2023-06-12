import React, { useState } from 'react';
import '../styles/Registerpage.css';

const RegisterPage = () => {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    vehicleType: '',
    vehicleChassisNumber: '',
    fuelType: '',
    vehicleNumberPlate: '',
  });

  const vehicleTypeOptions = [
    { label: 'Car', value: 'Car' },
    { label: 'Bus', value: 'Bus' },
    { label: 'Lorry', value: 'Lorry' },
    { label: 'Threewheel', value: 'Threewheel' },
    { label: 'Bike', value: 'Bike' },
  ];

  const fuelTypeOptions = [
    { label: 'Petrol', value: 'Petrol' },
    { label: 'Diesel', value: 'Diesel' },
  ];

  const [registrationSuccess, setRegistrationSuccess] = useState(false);

  const handleSubmit = async (e: { preventDefault: () => void; }) => {
    e.preventDefault();

    try {
      const response = await fetch('http://localhost:7222/api/Vehicle', {
         method: 'POST', 
         headers: { 'Content-Type': 'application/json' },
         body: JSON.stringify(formData) 
        }); 
        const jsonData = await response.json();

        if (response.ok) {
          setRegistrationSuccess(true);
          setFormData({
            firstName: '',
            lastName: '',
            email: '',
            vehicleType: '',
            vehicleChassisNumber: '',
            fuelType: '',
            vehicleNumberPlate: '',
          });
        } else {
          setRegistrationSuccess(false);
        }

    } catch (error) {
      console.error('Error occurred during registration', error);
      setRegistrationSuccess(false);
    }
  };

  const handleVehicleTypeChange = (e: { target: { value: any; }; }) => {
    setFormData((prevData) => ({
      ...prevData,
      vehicleType: e.target.value,
    }));
  };

  const handleFuelTypeChange = (e: { target: { value: any; }; }) => {
    setFormData((prevData) => ({
      ...prevData,
      fuelType: e.target.value,
    }));
  };

  const handleChange = (e: { target: { name: any; value: any; }; }) => {
    const { name, value } = e.target;

    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
};
  
  return (
    <div className="container">
      <h1 className="title">Registration Form</h1>
      {registrationSuccess && <p className="success-message">Successfully Registered</p>}
      <form onSubmit={handleSubmit} className="form">
       
        <div className="form-group">
          <label htmlFor="firstName" className="label">
            First Name
          </label>
          <input
            type="text"
            id="firstName"
            name="firstName"
            value={formData.firstName}
            onChange={handleChange}
            required
            className="input-field"
          />
        </div>
        <div className="form-group">
          <label htmlFor="lastName" className="label">
            Last Name
          </label>
          <input
            type="text"
            id="lastName"
            name="lastName"
            value={formData.lastName}
            onChange={handleChange}
            required
            className="input-field"
          />
        </div>

        <div className="form-group">
          <label htmlFor="email" className="label">
            Email
          </label>
          <input
            type="email"
            id="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            required
            className="input-field"
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="vehicleType" className="label">
            Vehicle Type
          </label>
          <select
            id="vehicleType"
            name="vehicleType"
            value={formData.vehicleType}
            onChange={handleVehicleTypeChange}
            required
            className="select-field"
          >
            <option value="">Select Vehicle Type</option>
            {vehicleTypeOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label htmlFor="vehicleChassisNumber" className="label">
            Vehicle Chassis Number
          </label>
          <input
            type="text"
            id="vehicleChassisNumber"
            name="vehicleChassisNumber"
            value={formData.vehicleChassisNumber}
            onChange={handleChange}
            required
            className="input-field"
          />
        </div>
        <div className="form-group">
          <label htmlFor="fuelType" className="label">
            Fuel Type
          </label>
          <select
            id="fuelType"
            name="fuelType"
            value={formData.fuelType}
            onChange={handleFuelTypeChange}
            required
            className="select-field"
          >
            <option value="">Select Fuel Type</option>
            {fuelTypeOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label htmlFor="vehicleNumberPlate" className="label">
            Vehicle Number Plate
          </label>
          <input
            type="text"
            id="vehicleNumberPlate"
            name="vehicleNumberPlate"
            value={formData.vehicleNumberPlate}
            onChange={handleChange}
            required
            className="input-field"
          />
        </div>
        <button type="submit" className="button">
          Submit
        </button>
      </form>
    </div>
  );
};

export default RegisterPage;
