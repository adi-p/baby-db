import './FetchAllForm.css';
import { useRef, useState } from "react";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faRepeat } from '@fortawesome/free-solid-svg-icons'
import { CONFIG } from "../Config";

export default function FetchAllForm() {
    const [keyValues, setKeyValues] = useState(null);
    const [fetchTime, setFetchTime] = useState(null);
    const [isUpdating, setIsUpdating] = useState(false);

    const intervalRef = useRef(null);

    const handleStartClick = (e) => {
      e.preventDefault();
      if(intervalRef.current) {
        return;
      }
      fetchKeyValues();
      intervalRef.current = setInterval(fetchKeyValues, 5000);
      setIsUpdating(true);
    }

    const handleStopClick = (e) => {
      e.preventDefault();
      clearInterval(intervalRef.current);
      intervalRef.current = null;
      setIsUpdating(false);
    }
  
    const fetchKeyValues = async () => {
      console.log(`Fetching All key value pairs`);

      setFetchTime(new Date());
  
      try {
        let response = await fetch(`${CONFIG.HOST}/values`);
  
        let data = await response.json();
        setKeyValues([...data])
      } catch(err) {
        console.log(err);
        // TODO: display error
      }
  
    }
  
    const handleFetchAllSubmit = (e) => {
      e.preventDefault();
      fetchKeyValues();
    }
  
    return (
      <div className="form-section">
        <h2> Fetch all keys and values </h2>
        <form onSubmit={handleFetchAllSubmit}>
          <button type="submit" className="primary-button"> Fetch All </button>
        </form>
        <form onSubmit={handleStartClick}>
          <button type="submit" className="primary-button"> Start Interval Fetch </button>
        </form>
        <form onSubmit={handleStopClick}>
          <button type="submit" className="secondary-button"> Stop Interval Fetch </button>
        </form>
        {keyValues && 
          <div>
            <h3> baby-db content: </h3>
            { isUpdating && <FontAwesomeIcon className="repeat-icon" icon={faRepeat} />}
            { fetchTime ? `Last updated at: ${fetchTime.toLocaleTimeString()}` : "No fetch has been made yet" }

            <pre className='code-block'>
              <code>
                { JSON.stringify(keyValues, null, 1) }
              </code>
            </pre>
          </div>
        }
      </div>
      
    );
  }
  