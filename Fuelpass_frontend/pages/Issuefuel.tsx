import React, { useState, useEffect } from 'react';
import axios from 'axios';
import '../styles/issueFuel.css';

interface VehicleDetails {
  VehicleType: string;
  TotalWeeklyQuota: number;
  RemainingQuota: number;
  VehicleNumberPlate: string;
}

interface IssueFuelProps {
  vehicleType: string;
}

const IssueFuel: React.FC<IssueFuelProps> = ({ vehicleType }) => {
  const [vehicleNumberPlate, setVehicleNumberPlate] = useState('');
  const [fuelAmount, setFuelAmount] = useState('');
  const [registrationStatus, setRegistrationStatus] = useState('');
  const [vehicleDetails, setVehicleDetails] = useState<VehicleDetails | null>(null);
  const [issueFuelStatus, setIssueFuelStatus] = useState('');

  useEffect(() => {
    if (vehicleDetails) {
      ////fetchWeeklyQuota("Car");
    }
  }, [vehicleDetails]);

  const checkVehicleRegistration = async () => {
    try {
      const response = await axios.get(`http://localhost:7222/api/vehiclenumber/${vehicleNumberPlate}`);

      if (response.status === 200 && response.data !== null) {
        // Vehicle is registered

        setRegistrationStatus('Vehicle number is registered');
        setVehicleDetails(response.data);
        fetchWeeklyQuota(vehicleNumberPlate)

      } else {
        // Vehicle is not registered
        setRegistrationStatus('Vehicle number not found');
        setVehicleDetails(null);
      }
    } catch (error) {
      console.error(error);
      setRegistrationStatus('Error occurred while checking vehicle registration');
      setVehicleDetails(null);
    }
  };

  const fetchWeeklyQuota = async (vehicleNumberPlate: string) => {
    try {
      //debugger
      const response = await axios.get(
        `http://localhost:7222/api/FuelApi/GetRemainingQuota/${vehicleNumberPlate}`
      );

      if (response.status === 200 && response.data !== null) {
        setVehicleDetails((prevState) => {
          if (prevState) {
            return {
              ...prevState,
              RemainingQuota: parseFloat(response.data), 
            };
          }
          return prevState;
        });
      }
    } catch (error) {
      console.error(error);
    }
  };


  const issueFuel = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!vehicleDetails) {
      return;
    }

    if (isNaN(parseFloat(fuelAmount)) || parseFloat(fuelAmount) <= 0) {
      setIssueFuelStatus('Fuel amount should be a positive number');
      return;
    }

    try {
      //debugger
      const response = await axios.put( 
        `http://localhost:7222/api/ReduceFuelQuota/${vehicleNumberPlate}`,
        {
          reductionAmount : parseFloat(fuelAmount),
        }
      );

      if (response.status === 200) {
        setIssueFuelStatus('Fuel issued successfully');

        setVehicleDetails((prevState) => {
          if (prevState) {
            return {
              ...prevState,
              RemainingQuota: prevState.RemainingQuota - parseFloat(fuelAmount),
            };
          }
          return prevState;
        });
      } else {
        setIssueFuelStatus('Failed to issue fuel');
      }
    } catch (error) {
      console.error(error);
      setIssueFuelStatus('Error occurred while issuing fuel');
    }
  };

  return (
    <div className="issue-fuel-container">
      <h1 className="issue-fuel-title">Issue Fuel</h1>
      <div className="registration-status">
        <input
          type="text"
          placeholder="Enter vehicle number plate"
          value={vehicleNumberPlate}
          onChange={(e) => setVehicleNumberPlate(e.target.value)}
        />
        <button onClick={checkVehicleRegistration}>Check Registration</button>
        {registrationStatus && <p>{registrationStatus}</p>}
      </div>
      {vehicleDetails && (
        <div className="vehicle-details">
          <p>
            Vehicle Type: <span>{vehicleDetails.VehicleType}</span>
          </p>
          
          <p>
          Remaining Quota: <span>{vehicleDetails?.RemainingQuota?.toString()}</span>
          </p>
        </div>
      )}

      {vehicleDetails && (
        <form onSubmit={issueFuel} className="fuel-issue-form">
          <div className="form-group">
            <label htmlFor="fuelAmount">Fuel Amount (Liters):</label>
            <input
              type="text"
              id="fuelAmount"
              value={fuelAmount}
              onChange={(e) => setFuelAmount(e.target.value)}
            />
          </div>
          <button type="submit">Issue Fuel</button>
        </form>
      )}
      {issueFuelStatus && <p>{issueFuelStatus}</p>}
    </div>
  );
};

export default IssueFuel;
